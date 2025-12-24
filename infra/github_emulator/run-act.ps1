#Requires -Version 7.0

<#
.SYNOPSIS
    Run GitHub Actions workflow locally using act with custom image.

.DESCRIPTION
    Provides an interactive way to test the main GitHub Actions workflow locally
    using the 'act' tool before pushing changes to GitHub. This eliminates the
    need to consume GitHub Actions minutes during development and debugging.

    The script handles:
    - Prerequisite validation (act CLI, Docker)
    - Interactive prompts with smart defaults
    - Secret file loading and validation
    - Comprehensive error handling with troubleshooting guidance

.PARAMETER Environment
    Target deployment environment (dev or prod).
    Default: Prompts interactively if not provided.

.PARAMETER SkipTerraform
    Skip Terraform infrastructure deployment.
    Default: Prompts interactively (default: yes).

.PARAMETER SkipDatabases
    Skip database deployment.
    Default: Prompts interactively (default: no).

.PARAMETER RecreateDatabase
    Recreate all database objects (drops and recreates).
    Only allowed in dev environment.
    Default: Prompts interactively (default: no).

.PARAMETER LogVerbosity
    Log verbosity level (normal or debug).
    Default: Prompts interactively (default: normal).

.PARAMETER NonInteractive
    Run without prompts (requires all parameters).
    Useful for scripting/automation.

.PARAMETER NoWait
    Don't wait for keypress on exit (even on errors).
    Useful for automation.

.PARAMETER RebuildImage
    Force rebuild of the custom Docker runner image even if it exists.
    Useful when updating the Dockerfile or when tools need updating.
    Default: Prompts for confirmation if image missing (no rebuild if exists).

.EXAMPLE
    .\run-act.ps1

    Interactive mode (default). Prompts for all configuration options.

.EXAMPLE
    .\run-act.ps1 -Environment dev -SkipTerraform

    Partially parameterized - prompts for remaining options.

.EXAMPLE
    .\run-act.ps1 -Environment dev -SkipTerraform -SkipDatabases -NonInteractive

    Fully automated mode with no prompts.

.EXAMPLE
    .\run-act.ps1 -Verbose

    Interactive mode with detailed verbose output.

.NOTES
    Prerequisite: act CLI (https://github.com/nektos/act)
    Prerequisite: Docker Desktop (Windows/Mac) or Docker Engine (Linux)

    Install act:
        Windows (Chocolatey):  choco install act-cli
        Windows (Scoop):       scoop install act
        Linux/macOS:           brew install act

.LINK
    https://github.com/nektos/act
#>

[CmdletBinding()]
param(
    [ValidateSet('dev', 'prod')]
    [string]$Environment,

    [switch]$SkipTerraform,

    [switch]$SkipDatabases,

    [switch]$RecreateDatabase,

    [ValidateSet('normal', 'debug')]
    [string]$LogVerbosity = 'normal',

    [switch]$NonInteractive,

    [switch]$NoWait,

    [switch]$RebuildImage
)

# ==============================
# SCRIPT INITIALIZATION
# ==============================
$ErrorActionPreference = "Stop"
$ScriptRoot = $PSScriptRoot
$RepoRoot = Resolve-Path "$ScriptRoot\..\.."

# Configuration constants
$WorkflowFile = ".github\workflows\main.yml"
$ActSecretFile = Join-Path $RepoRoot "infra\github_emulator\.env.act"
$DefaultLogVerbosity = "normal"

# Docker image configuration
$LocalImageName = "consilientwebapp-runner"
$LocalImageTag = "latest"
$LocalImageFull = "${LocalImageName}:${LocalImageTag}"
$DockerfilePath = Join-Path $RepoRoot ".github\workflows\runner\Dockerfile"
$DockerContextPath = Join-Path $RepoRoot ".github\workflows\runner"

# Cloud image (GHCR) - for reference/documentation only
$CloudImageReference = "ghcr.io/your-org/consilientwebapp/actions-runner:latest"

# ==============================
# PREREQUISITE VALIDATION FUNCTIONS
# ==============================

function Test-ActInstalled {
    <#
    .SYNOPSIS
        Verify act CLI is installed and accessible.
    #>
    Write-Host "  Checking act CLI..." -ForegroundColor Gray

    try {
        $output = & act --version 2>&1
        if ($LASTEXITCODE -ne 0) {
            return $false
        }
        Write-Host "  ✅ act CLI found ($($output.Trim()))" -ForegroundColor Green
        return $true
    }
    catch {
        return $false
    }
}

function Test-DockerRunning {
    <#
    .SYNOPSIS
        Verify Docker daemon is running and accessible.
    #>
    Write-Host "  Checking Docker..." -ForegroundColor Gray

    try {
        $output = & docker ps 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            return $false
        }
        Write-Host "  ✅ Docker running" -ForegroundColor Green
        return $true
    }
    catch {
        return $false
    }
}

function Test-SecretFile {
    <#
    .SYNOPSIS
        Check if secret file exists and return path or null.
    #>
    Write-Host "  Checking secret file..." -ForegroundColor Gray

    if (Test-Path $ActSecretFile) {
        Write-Host "  ✅ Secret file found" -ForegroundColor Green
        return $ActSecretFile
    }
    else {
        Write-Host "  ⚠️ Secret file not found ($ActSecretFile)" -ForegroundColor Yellow
        return $null
    }
}

function Test-DockerImageExists {
    <#
    .SYNOPSIS
        Verify that the custom runner Docker image exists locally.

    .DESCRIPTION
        Checks if the custom GitHub Actions runner image is available
        in the local Docker daemon.

    .PARAMETER ImageName
        The full image name in format "name:tag" (e.g., "consilientwebapp-runner:latest")
    #>
    param(
        [string]$ImageName
    )

    Write-Host "  Checking Docker image..." -ForegroundColor Gray

    try {
        $images = & docker images --format "{{.Repository}}:{{.Tag}}" 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  ⚠️ Unable to list Docker images" -ForegroundColor Yellow
            return $false
        }

        if ($images -contains $ImageName) {
            Write-Host "  ✅ Custom runner image found ($ImageName)" -ForegroundColor Green
            return $true
        }
        else {
            Write-Host "  ⚠️ Custom runner image not found ($ImageName)" -ForegroundColor Yellow
            return $false
        }
    }
    catch {
        Write-Host "  ⚠️ Error checking Docker images" -ForegroundColor Yellow
        return $false
    }
}

function Build-DockerImage {
    <#
    .SYNOPSIS
        Build the custom runner Docker image locally.

    .DESCRIPTION
        Builds the custom GitHub Actions runner image from the Dockerfile
        in .github/workflows/runner/. Shows progress and handles errors gracefully.

    .PARAMETER ImageName
        The name:tag for the Docker image to build.

    .PARAMETER DockerfilePath
        Path to the Dockerfile.

    .PARAMETER ContextPath
        Path to the Docker build context directory.
    #>
    param(
        [string]$ImageName,
        [string]$DockerfilePath,
        [string]$ContextPath
    )

    Write-Host ""
    Write-Host "🔨 Building custom runner image..." -ForegroundColor Cyan
    Write-Host "  Image: $ImageName" -ForegroundColor Gray
    Write-Host "  Dockerfile: $DockerfilePath" -ForegroundColor Gray
    Write-Host ""

    # Verify Dockerfile exists
    if (-not (Test-Path $DockerfilePath)) {
        throw "Dockerfile not found at: $DockerfilePath"
    }

    Write-Host "This may take a few minutes (downloading base image, installing tools)..." -ForegroundColor Yellow
    Write-Host ""

    try {
        # Build with progress output
        $buildArgs = @(
            "build",
            "-t", $ImageName,
            "-f", $DockerfilePath,
            $ContextPath
        )

        & docker $buildArgs

        if ($LASTEXITCODE -ne 0) {
            throw "Docker build failed with exit code $LASTEXITCODE"
        }

        Write-Host ""
        Write-Host "✅ Docker image built successfully: $ImageName" -ForegroundColor Green
        Write-Host ""

        return $true
    }
    catch {
        Write-Host ""
        Write-Host "❌ Failed to build Docker image" -ForegroundColor Red
        Write-Host ""
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
        Write-Host "Troubleshooting:" -ForegroundColor Yellow
        Write-Host "  1. Verify Docker has enough disk space (check Docker Desktop settings)" -ForegroundColor Gray
        Write-Host "  2. Check network connectivity (downloads base Ubuntu image)" -ForegroundColor Gray
        Write-Host "  3. Review Dockerfile at: $DockerfilePath" -ForegroundColor Gray
        Write-Host "  4. Try building manually:" -ForegroundColor Gray
        Write-Host "     docker build -t $ImageName -f $DockerfilePath $ContextPath" -ForegroundColor White
        Write-Host ""

        throw $_
    }
}

# ==============================
# USER INTERACTION FUNCTIONS
# ==============================

function Get-ValidatedInput {
    <#
    .SYNOPSIS
        Get user input with validation and colored output.

    .PARAMETER Prompt
        The prompt text to display.

    .PARAMETER Default
        Default value if user presses enter without input.

    .PARAMETER AllowedValues
        Array of allowed values for validation.
    #>
    param(
        [string]$Prompt,
        [string]$Default = $null,
        [string[]]$AllowedValues = $null
    )

    while ($true) {
        $displayPrompt = if ($Default) { "$Prompt [$Default]: " } else { "$Prompt`: " }
        $input = Read-Host $displayPrompt

        if ([string]::IsNullOrWhiteSpace($input) -and $Default) {
            $input = $Default
        }

        if ([string]::IsNullOrWhiteSpace($input)) {
            Write-Host "Input is required." -ForegroundColor Red
            continue
        }

        if ($AllowedValues -and $AllowedValues.Count -gt 0 -and $input -notin $AllowedValues) {
            Write-Host "Invalid input. Allowed values: $($AllowedValues -join ', ')" -ForegroundColor Red
            continue
        }

        Write-Host "✅ Selected: $input" -ForegroundColor Green
        return $input
    }
}

function Show-ExecutionSummary {
    <#
    .SYNOPSIS
        Display configuration summary before execution.
    #>
    param(
        [string]$Environment,
        [string]$SkipTf,
        [string]$SkipDb,
        [string]$RecreateDb,
        [string]$LogLevel,
        [string]$SecretFile
    )

    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  GitHub Actions Local Emulator" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Configuration Summary:" -ForegroundColor Yellow
    Write-Host "  Workflow: main.yml" -ForegroundColor Gray
    Write-Host "  Environment: $Environment" -ForegroundColor Gray
    Write-Host "  Skip Terraform: $(if($SkipTf -eq 'true') {'Yes'} else {'No'})" -ForegroundColor Gray
    Write-Host "  Skip Database: $(if($SkipDb -eq 'true') {'Yes'} else {'No'})" -ForegroundColor Gray
    Write-Host "  Recreate DB Objects: $(if($RecreateDb -eq 'true') {'Yes'} else {'No'})" -ForegroundColor Gray
    Write-Host "  Log Verbosity: $LogLevel" -ForegroundColor Gray

    if ($SecretFile) {
        Write-Host "  Secret File: Found" -ForegroundColor Gray
    }
    else {
        Write-Host "  Secret File: Not found (will continue without secrets)" -ForegroundColor Gray
    }

    Write-Host ""
}

# ==============================
# ACT EXECUTION FUNCTIONS
# ==============================

function Invoke-ActExecution {
    <#
    .SYNOPSIS
        Execute act command with error handling.
    #>
    param(
        [string[]]$ActArgs
    )

    Write-Host "Running act with custom image ($LocalImageFull)..." -ForegroundColor Cyan
    Write-Host ""

    try {
        & act $ActArgs
        if ($LASTEXITCODE -ne 0) {
            throw "act exited with code $LASTEXITCODE"
        }
        return $true
    }
    catch {
        throw $_
    }
}

# ==============================
# ERROR HANDLING HELPERS
# ==============================

function Show-ActNotInstalledError {
    Write-Host ""
    Write-Host "❌ Error: act CLI not found" -ForegroundColor Red
    Write-Host ""
    Write-Host "The 'act' tool is required to run GitHub Actions workflows locally." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Install with:" -ForegroundColor Yellow
    Write-Host "  Windows (Chocolatey):  choco install act-cli" -ForegroundColor White
    Write-Host "  Windows (Scoop):       scoop install act" -ForegroundColor White
    Write-Host "  Linux/macOS:           brew install act" -ForegroundColor White
    Write-Host "  Manual:                https://github.com/nektos/act/releases" -ForegroundColor White
    Write-Host ""
    Write-Host "After installation, restart your terminal and try again." -ForegroundColor Gray
}

function Show-DockerNotRunningError {
    Write-Host ""
    Write-Host "❌ Error: Docker is not running" -ForegroundColor Red
    Write-Host ""
    Write-Host "GitHub Actions workflows run in Docker containers via act." -ForegroundColor Yellow
    Write-Host "Docker must be running before executing this script." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "  Windows/Mac:  Start Docker Desktop" -ForegroundColor Gray
    Write-Host "  Linux:        sudo systemctl start docker" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Verify with:" -ForegroundColor Yellow
    Write-Host "  docker ps" -ForegroundColor White
    Write-Host ""
    Write-Host "If Docker Desktop is installed but not running:" -ForegroundColor Gray
    Write-Host "  1. Open Docker Desktop application" -ForegroundColor Gray
    Write-Host "  2. Wait for the whale icon to stabilize" -ForegroundColor Gray
    Write-Host "  3. Verify with 'docker ps' in a new terminal" -ForegroundColor Gray
    Write-Host "  4. Try this script again" -ForegroundColor Gray
}

function Show-InvalidParamError {
    param([string]$Message)

    Write-Host ""
    Write-Host "❌ Error: $Message" -ForegroundColor Red
    Write-Host ""
}

function Show-ActExecutionError {
    Write-Host ""
    Write-Host "❌ Error: Workflow execution failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "The act command failed. Check the output above for details." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Possible causes:" -ForegroundColor Yellow
    Write-Host "  - Workflow syntax error in .github/workflows/main.yml" -ForegroundColor Gray
    Write-Host "  - Docker image pull failed (network issue)" -ForegroundColor Gray
    Write-Host "  - Missing required secrets in .env.act" -ForegroundColor Gray
    Write-Host "  - Insufficient Docker resources" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Troubleshooting steps:" -ForegroundColor Yellow
    Write-Host "  1. Run with -Verbose for more details" -ForegroundColor Gray
    Write-Host "  2. Check .github/workflows/main.yml syntax" -ForegroundColor Gray
    Write-Host "  3. Verify secrets in infra/github_emulator/.env.act" -ForegroundColor Gray
    Write-Host "  4. Check Docker Desktop resources (memory, disk space)" -ForegroundColor Gray
}

# ==============================
# MAIN EXECUTION
# ==============================

try {
    Push-Location $RepoRoot

    # ==============================
    # PREREQUISITE CHECKS
    # ==============================
    Write-Host "🔍 Checking prerequisites..." -ForegroundColor Yellow
    Write-Host ""

    if (-not (Test-ActInstalled)) {
        Show-ActNotInstalledError
        exit 10
    }

    if (-not (Test-DockerRunning)) {
        Show-DockerNotRunningError
        exit 11
    }

    $secretFile = Test-SecretFile

    # Check if custom runner image exists, build if missing or if rebuild requested
    $needsBuild = (-not (Test-DockerImageExists -ImageName $LocalImageFull)) -or $RebuildImage

    if ($needsBuild) {
        if ($RebuildImage) {
            Write-Host "Force rebuild requested for custom runner image." -ForegroundColor Yellow
        }
        else {
            Write-Host "Custom runner image not found locally." -ForegroundColor Yellow
            Write-Host "The image needs to be built before running act." -ForegroundColor Yellow
        }
        Write-Host ""
        Write-Host "Building image: $LocalImageFull" -ForegroundColor Cyan
        Write-Host "  From: $DockerfilePath" -ForegroundColor Gray
        Write-Host ""

        # Optional: Prompt for confirmation in interactive mode (unless forced rebuild)
        if (-not $NonInteractive -and -not $RebuildImage) {
            $buildConfirm = Get-ValidatedInput "Build the Docker image now? (y/n)" "y" @("y", "n")
            if ($buildConfirm -ne "y") {
                Write-Host ""
                Write-Host "❌ Cannot proceed without the custom runner image" -ForegroundColor Red
                Write-Host ""
                Write-Host "To build manually, run:" -ForegroundColor Yellow
                Write-Host "  docker build -t $LocalImageFull -f $DockerfilePath $DockerContextPath" -ForegroundColor White
                Write-Host ""
                exit 12
            }
        }

        # Build the image
        try {
            Build-DockerImage -ImageName $LocalImageFull -DockerfilePath $DockerfilePath -ContextPath $DockerContextPath
        }
        catch {
            Write-Host "Failed to build Docker image. Cannot proceed." -ForegroundColor Red
            exit 13
        }
    }
    else {
        Write-Host "✅ Custom runner image found ($LocalImageFull)" -ForegroundColor Green
    }

    Write-Host ""

    # ==============================
    # PARAMETER VALIDATION
    # ==============================

    # Validate RecreateDatabase only in dev
    if ($RecreateDatabase -and $Environment -eq 'prod') {
        Show-InvalidParamError "Cannot recreate database objects in production"
        Write-Host "Recreating database objects (drops all tables, views, procedures)" -ForegroundColor Yellow
        Write-Host "is only allowed in the 'dev' environment for safety reasons." -ForegroundColor Yellow
        Write-Host ""
        Write-Host "For production database changes:" -ForegroundColor Yellow
        Write-Host "  1. Test in dev environment first" -ForegroundColor Gray
        Write-Host "  2. Create migration scripts" -ForegroundColor Gray
        Write-Host "  3. Review and approve changes" -ForegroundColor Gray
        Write-Host "  4. Deploy via controlled migration process" -ForegroundColor Gray
        exit 20
    }

    # ==============================
    # GATHER CONFIGURATION
    # ==============================

    $EnvInput = $Environment
    $SkipTfInput = $SkipTerraform
    $SkipDbInput = $SkipDatabases
    $RecreateDbInput = $RecreateDatabase
    $LogVerbosityInput = $LogVerbosity

    # Interactive mode: Prompt for missing values
    if (-not $NonInteractive) {
        Write-Host "📋 Configuration" -ForegroundColor Yellow
        Write-Host ""

        if (-not $EnvInput) {
            $EnvInput = Get-ValidatedInput "Enter environment (dev/prod)" "dev" @("dev", "prod")
        }
        else {
            Write-Host "Environment: $EnvInput" -ForegroundColor Green
        }

        if (-not $SkipTfInput) {
            $SkipTfResponse = Get-ValidatedInput "Skip Terraform deployment? (y/n)" "y" @("y", "n")
            $SkipTfInput = $SkipTfResponse -eq "y"
        }

        Write-Host "Terraform will be: $(if($SkipTfInput) {'SKIPPED'} else {'EXECUTED'})" -ForegroundColor $(if($SkipTfInput) {'Yellow'} else {'Yellow'})

        if (-not $SkipDbInput) {
            $SkipDbResponse = Get-ValidatedInput "Skip Database deployment? (y/n)" "n" @("y", "n")
            $SkipDbInput = $SkipDbResponse -eq "y"
        }

        Write-Host "Database deployment will be: $(if($SkipDbInput) {'SKIPPED'} else {'EXECUTED'})" -ForegroundColor $(if($SkipDbInput) {'Yellow'} else {'Yellow'})
        Write-Host ""

        # Database-specific prompts only if databases are not skipped
        if (-not $SkipDbInput) {
            if (-not $RecreateDbInput) {
                $RecreateDbResponse = Get-ValidatedInput "Recreate all database objects? (y/n)" "n" @("y", "n")
                $RecreateDbInput = $RecreateDbResponse -eq "y"
            }

            if ($RecreateDbInput) {
                Write-Host "Database objects will be RECREATED (drops all objects first)" -ForegroundColor Red
            }
            else {
                Write-Host "Database objects will NOT be recreated" -ForegroundColor Green
            }

            Write-Host ""
        }

        # Log verbosity
        if ($LogVerbosityInput -eq 'normal') {
            $LogInput = Get-ValidatedInput "Log verbosity level (normal/debug)" $DefaultLogVerbosity @("normal", "debug")
            $LogVerbosityInput = $LogInput
        }

        Write-Host "Log verbosity: $LogVerbosityInput" -ForegroundColor Green
        Write-Host ""
    }
    else {
        # Non-interactive mode: Validate all required parameters are provided
        if (-not $EnvInput) {
            Show-InvalidParamError "NonInteractive mode requires -Environment parameter"
            exit 20
        }
    }

    # Convert boolean switches to string format for act
    $SkipTerraformStr = if ($SkipTfInput) { "true" } else { "false" }
    $SkipDatabasesStr = if ($SkipDbInput) { "true" } else { "false" }
    $RecreateDbStr = if ($RecreateDbInput) { "true" } else { "false" }

    # ==============================
    # SHOW SUMMARY
    # ==============================

    Show-ExecutionSummary -Environment $EnvInput -SkipTf $SkipTerraformStr -SkipDb $SkipDatabasesStr -RecreateDb $RecreateDbStr -LogLevel $LogVerbosityInput -SecretFile $secretFile

    # ==============================
    # BUILD ACT COMMAND
    # ==============================

    $ActArgs = @(
        "workflow_dispatch",
        "--input", "environment=$EnvInput",
        "--input", "skip_terraform=$SkipTerraformStr",
        "--input", "skip_databases=$SkipDatabasesStr",
        "--input", "recreate_database_objects=$RecreateDbStr",
        "--input", "log_verbosity=$LogVerbosityInput",
        "-W", $WorkflowFile,
        "-P", "ubuntu-latest=$LocalImageFull"
    )

    # Add secret file if it exists
    if ($secretFile) {
        $ActArgs += "--secret-file"
        $ActArgs += $secretFile
    }

    # ==============================
    # EXECUTE ACT
    # ==============================

    Invoke-ActExecution $ActArgs

    Write-Host ""
    Write-Host "✅ Done!" -ForegroundColor Green
    Write-Host ""
}
catch {
    if ($VerbosePreference -eq 'Continue') {
        Write-Host ""
        Write-Host "Stack trace:" -ForegroundColor DarkGray
        Write-Host $_.ScriptStackTrace -ForegroundColor DarkGray
    }

    # Show specific error message if it was our custom error
    if ($_.Exception.Message.Contains("Workflow execution failed") -or $_.Exception.Message.Contains("act exited")) {
        Show-ActExecutionError
    }

    if (-not $NoWait) {
        Write-Host ""
        Write-Host "Press any key to exit..." -ForegroundColor Gray
        $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    }

    exit 1
}
finally {
    Pop-Location -ErrorAction SilentlyContinue
}
