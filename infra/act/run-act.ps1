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
    - Interactive prompts with hierarchical, positive language ("Run X?" defaults)
    - Secret file loading and validation
    - Custom runner Docker image management
    - Comprehensive error handling with troubleshooting guidance

.PARAMETER Environment
    Target deployment environment (dev or prod).
    Default: Prompts interactively if not provided.

.PARAMETER RunTerraform
    Run Terraform infrastructure deployment.
    Default: Prompts interactively (default: yes).

.PARAMETER AddFirewallRule
    Add local firewall rule for Azure resources (only when Terraform is enabled).
    Default: Prompts interactively (default: yes).

.PARAMETER RunDatabases
    Run database deployment.
    Default: Prompts interactively (default: yes).

.PARAMETER RecreateDatabase
    Recreate all database objects (drops and recreates).
    Only allowed in dev environment. Sub-prompt under RunDatabases.
    Default: Prompts interactively (default: yes).

.PARAMETER RunDbDocs
    Generate database documentation using SchemaSpy.
    Sub-prompt under RunDatabases.
    Default: Prompts interactively (default: yes).

.PARAMETER RunApiDeployment
    Run .NET App deployment.
    Default: Prompts interactively (default: yes).

.PARAMETER RunReactDeployment
    Run React deployment.
    Default: Prompts interactively (default: yes).

.PARAMETER RunHealthChecks
    Run health checks (only if apps are being deployed).
    Conditional prompt shown only when RunApiDeployment or RunReactDeployment is true.
    Default: Prompts interactively (default: yes).

.PARAMETER RecreateImage
    Force rebuild of the custom Docker runner image even if it exists.
    Destructive operation - default is NO. Use when updating Dockerfile or tools.
    Default: No (false) - only rebuild if missing.

.PARAMETER RecreateCache
    Force recreation of the act actions cache.
    Destructive operation - default is NO. Use to clear cached actions for clean rebuild.
    Default: No (false) - preserve existing cache.

.PARAMETER NonInteractive
    Run without prompts (requires all parameters).
    Useful for scripting/automation.

.PARAMETER NoWait
    Don't wait for keypress on exit (even on errors).
    Useful for automation.

.PARAMETER LogLevel
    Control output verbosity level.
    Valid values: 'Verbose' (show all output), 'Normal' (minimal output)
    Default: 'Verbose' (shows all output including docker operations)

.EXAMPLE
    .\run-act.ps1

    Interactive mode (default). Prompts for all configuration options with sensible defaults.

.EXAMPLE
    .\run-act.ps1 -Environment dev

    Partially parameterized with environment - prompts for remaining deployment options.

.EXAMPLE
    .\run-act.ps1 -Environment dev -RunTerraform -RunDatabases -RecreateDatabase -RunApiDeployment -RunReactDeployment -NonInteractive

    Fully automated mode with all deployments enabled.

.EXAMPLE
    .\run-act.ps1 -Environment dev -RunTerraform:$false -RunDatabases:$false -NonInteractive

    Fully automated mode - skip Terraform and database deployments, deploy apps only.

.EXAMPLE
    .\run-act.ps1 -RecreateImage -RecreateCache

    Interactive mode with fresh Docker image and actions cache rebuild.

.EXAMPLE
    .\run-act.ps1 -Verbose

    Interactive mode with detailed verbose output.

.EXAMPLE
    .\run-act.ps1 -Environment dev -NonInteractive -Info

    Fully automated mode with info-level output (for testing).

.NOTES
    Prerequisite: act CLI (https://github.com/nektos/act)
    Prerequisite: Docker Desktop (Windows/Mac) or Docker Engine (Linux)

    Install act:
        Windows (Chocolatey):  choco install act-cli
        Windows (Scoop):       scoop install act
        Linux/macOS:           brew install act

.LINK
    https://github.com/nektos/act

.AI-DOCS
    For AI assistants: This script is part of the local GitHub Actions testing system.
    Primary Documentation: docs/infra/components/local-testing.md
    GitHub Actions Overview: docs/infra/components/github-actions.md
    Configuration Files: .env.act (secrets, git-ignored)
    Related Workflows: .github/workflows/main.yml (orchestrator)
    Custom Runner: .github/workflows/runner/Dockerfile
    Docker Build Logic: infra/act/Build-RunnerImage.ps1
#>

[CmdletBinding()]
param(
    [ValidateSet('dev', 'prod')]
    [string]$Environment,

    [switch]$RunTerraform,

    [switch]$AddFirewallRule,

    [switch]$RunDatabases,

    [switch]$RecreateDatabase,

    [switch]$RunDbDocs,

    [switch]$RunApiDeployment,

    [switch]$RunReactDeployment,

    [switch]$RunHealthChecks,

    [switch]$RecreateImage,

    [switch]$RecreateCache,

    [switch]$NonInteractive,

    [switch]$NoWait,

    [ValidateSet('Normal', 'Verbose')]
    [string]$LogLevel = 'Verbose'
)

# ==============================
# SCRIPT INITIALIZATION
# ==============================
$ErrorActionPreference = "Stop"
$ScriptRoot = $PSScriptRoot
$RepoRoot = Resolve-Path "$ScriptRoot\..\.."

# ==============================
# OUTPUT HELPER FUNCTIONS
# ==============================

# Import shared Write-Message helper
$WriteMessagePath = Join-Path $ScriptRoot "lib\Write-Message.ps1"
if (Test-Path $WriteMessagePath) {
    . $WriteMessagePath
}
else {
    Write-Host "Error: Write-Message helper not found at: $WriteMessagePath" -ForegroundColor Red
    exit 1
}

function ConvertTo-ActBooleanString {
    <#
    .SYNOPSIS
        Convert PowerShell boolean to act command input string format.
    .DESCRIPTION
        Converts $true/$false to "true"/"false" strings for act workflow inputs.
    .PARAMETER Value
        The boolean value to convert.
    #>
    param([bool]$Value)
    if ($Value) { "true" } else { "false" }
}


# Configuration constants
# For AI: Main workflow orchestrator file - see docs/infra/components/github-actions.md
$WorkflowFile = ".github\workflows\main.yml"

# For AI: .env.act contains all secrets and credentials (git-ignored)
# See docs/infra/components/local-testing.md#secrets-management
$ActSecretFile = Join-Path $ScriptRoot ".env.act"

# Docker image configuration (also defined in Build-RunnerImage.ps1)
# For AI: Custom runner image with pre-installed tools (Azure CLI, sqlcmd, Terraform, etc.)
# See docs/infra/components/github-actions.md#custom-runner-image
$LocalImageName = "consilientwebapp-runner"
$LocalImageTag = "latest"
$LocalImageFull = "${LocalImageName}:${LocalImageTag}"

# ==============================
# PREREQUISITE VALIDATION FUNCTIONS
# ==============================

function Test-ActInstalled {
    <#
    .SYNOPSIS
        Verify act CLI is installed and accessible.
    #>
    Write-Message -LogLevel $LogLevel -Level Info -Message "  Checking act CLI..."

    try {
        $output = & act --version 2>&1
        if ($LASTEXITCODE -ne 0) {
            return $false
        }
        Write-Message -LogLevel $LogLevel -Level Debug -Message "  ✅ act CLI found ($($output.Trim()))"
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
    Write-Message -LogLevel $LogLevel -Level Info -Message "  Checking secret file..."

    if (Test-Path $ActSecretFile) {
        Write-Message -LogLevel $LogLevel -Level Debug -Message "  ✅ Secret file found"
        return $ActSecretFile
    }
    else {
        Write-Message -LogLevel $LogLevel -Level Warning -Message "  ⚠️ Secret file not found ($ActSecretFile)"
        return $null
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
        if ($Default) {
            Write-Host -NoNewline ($Prompt + " [" + $Default + "]: ")
        } else {
            Write-Host -NoNewline ($Prompt + ": ")
        }

        # Read input character by character and echo it back
        $userInput = ""
        while ($true) {
            $key = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
            if ($key.VirtualKeyCode -eq 13) {  # Enter key
                break
            }
            elseif ($key.VirtualKeyCode -eq 8) {  # Backspace key
                if ($userInput.Length -gt 0) {
                    $userInput = $userInput.Substring(0, $userInput.Length - 1)
                    Write-Host -NoNewline "`b `b"
                }
            }
            else {
                $userInput += $key.Character
                Write-Host -NoNewline $key.Character
            }
        }

        $isDefault = [string]::IsNullOrWhiteSpace($userInput)
        if ($isDefault -and $Default) {
            $userInput = $Default
            Write-Host -NoNewline $userInput
        }

        Write-Host ""

        if ([string]::IsNullOrWhiteSpace($userInput)) {
            Write-Host "Input is required." -ForegroundColor Red
            continue
        }

        if ($AllowedValues -and $AllowedValues.Count -gt 0 -and $userInput -notin $AllowedValues) {
            Write-Host "Invalid input. Allowed values: $($AllowedValues -join ', ')" -ForegroundColor Red
            continue
        }

        return $userInput
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
        [string]$SecretFile,
        [string]$AllowFirewall,
        [string]$SkipHealthChecks
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
    Write-Host "  Allow Local Firewall: $(if($AllowFirewall -eq 'true') {'Yes (INSECURE - dev only)'} else {'No'})" -ForegroundColor $(if($AllowFirewall -eq 'true') {'Yellow'} else {'Gray'})
    Write-Host "  Skip Health Checks: $(if($SkipHealthChecks -eq 'true') {'Yes'} else {'No'})" -ForegroundColor Gray

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

    Write-Message -LogLevel $LogLevel -Level Step -Message "Running act with custom image ($LocalImageFull)..."

    try {
        & act $ActArgs 2>&1
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
    Write-Message -LogLevel $LogLevel -Level Error -Message "❌ Error: act CLI not found"
    Write-Host ""
    Write-Message -LogLevel $LogLevel -Level Warning -Message "The 'act' tool is required to run GitHub Actions workflows locally."
    Write-Host ""
    Write-Message -LogLevel $LogLevel -Level Warning -Message "Install with:"
    Write-Host "  Windows (Chocolatey):  choco install act-cli" -ForegroundColor White
    Write-Host "  Windows (Scoop):       scoop install act" -ForegroundColor White
    Write-Host "  Linux/macOS:           brew install act" -ForegroundColor White
    Write-Host "  Manual:                https://github.com/nektos/act/releases" -ForegroundColor White
    Write-Host ""
    Write-Message -LogLevel $LogLevel -Level Info -Message "After installation, restart your terminal and try again."
}

function Show-DockerNotRunningError {
    Write-Host ""
    Write-Message -LogLevel $LogLevel -Level Error -Message "❌ Error: Docker is not running"
    Write-Host ""
    Write-Message -LogLevel $LogLevel -Level Warning -Message "GitHub Actions workflows run in Docker containers via act."
    Write-Message -LogLevel $LogLevel -Level Warning -Message "Docker must be running before executing this script."
    Write-Host ""
    Write-Message -LogLevel $LogLevel -Level Warning -Message "Troubleshooting:"
    Write-Host "  Windows/Mac:  Start Docker Desktop" -ForegroundColor Gray
    Write-Host "  Linux:        sudo systemctl start docker" -ForegroundColor Gray
    Write-Host ""
    Write-Message -LogLevel $LogLevel -Level Warning -Message "Verify with:"
    Write-Host "  docker ps" -ForegroundColor White
    Write-Host ""
    Write-Message -LogLevel $LogLevel -Level Info -Message "If Docker Desktop is installed but not running:"
    Write-Host "  1. Open Docker Desktop application" -ForegroundColor Gray
    Write-Host "  2. Wait for the whale icon to stabilize" -ForegroundColor Gray
    Write-Host "  3. Verify with 'docker ps' in a new terminal" -ForegroundColor Gray
    Write-Host "  4. Try this script again" -ForegroundColor Gray
}

function Show-InvalidParamError {
    param([string]$Message)

    Write-Host ""
    Write-Message -LogLevel $LogLevel -Level Error -Message "❌ Error: $Message"
    Write-Host ""
}

function Show-ActExecutionError {
    Write-Host ""
    Write-Message -LogLevel $LogLevel -Level Error -Message "❌ Error: Workflow execution failed"
    Write-Host ""
    Write-Message -LogLevel $LogLevel -Level Warning -Message "The act command failed. Check the output above for details."
    Write-Host ""
    Write-Message -LogLevel $LogLevel -Level Warning -Message "Possible causes:"
    Write-Host "  - Workflow syntax error in .github/workflows/main.yml" -ForegroundColor Gray
    Write-Host "  - Docker image pull failed (network issue)" -ForegroundColor Gray
    Write-Host "  - Missing required secrets in .env.act" -ForegroundColor Gray
    Write-Host "  - Insufficient Docker resources" -ForegroundColor Gray
    Write-Host ""
    Write-Message -LogLevel $LogLevel -Level Warning -Message "Troubleshooting steps:"
    Write-Host "  1. Run with -Verbose for more details" -ForegroundColor Gray
    Write-Host "  2. Check .github/workflows/main.yml syntax" -ForegroundColor Gray
    Write-Host "  3. Verify secrets in infra/act/.env.act" -ForegroundColor Gray
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
    Write-Message -LogLevel $LogLevel -Level Info -Message "🔍 Checking prerequisites..."
    Write-Host ""

    if (-not (Test-ActInstalled)) {
        Show-ActNotInstalledError
        exit 10
    }

    $secretFile = Test-SecretFile

    # Build or verify custom runner Docker image
    # (Build-RunnerImage.ps1 will handle all Docker diagnostics)
    $buildImageScript = Join-Path $PSScriptRoot "Build-RunnerImage.ps1"
    if (-not (Test-Path $buildImageScript)) {
        Write-Message -LogLevel $LogLevel -Level Error -Message "Build script not found: $buildImageScript"
        exit 14
    }

    $buildParams = @{
        Force = $RebuildImage
        LogLevel = $LogLevel
    }
    if ($VerbosePreference -eq 'Continue') {
        $buildParams['Verbose'] = $true
    }
    if ($NonInteractive) {
        $buildParams['NonInteractive'] = $true
    }

    try {
        & $buildImageScript @buildParams
        if ($LASTEXITCODE -ne 0) {
            Write-Message -LogLevel $LogLevel -Level Error -Message "Failed to build or verify runner image"
            exit 15
        }
    }
    catch {
        Write-Message -LogLevel $LogLevel -Level Error -Message "Error running Build-RunnerImage.ps1: $_"
        exit 16
    }


    # ==============================
    # PARAMETER VALIDATION
    # ==============================

    # Validate RecreateDatabase only in dev
    if ($RecreateDatabase -and $Environment -eq 'prod') {
        Show-InvalidParamError "Cannot recreate database objects in production"
        Write-Message -LogLevel $LogLevel -Level Warning -Message "Recreating database objects (drops all tables, views, procedures)"
        Write-Message -LogLevel $LogLevel -Level Warning -Message "is only allowed in the 'dev' environment for safety reasons."
        Write-Host ""
        Write-Message -LogLevel $LogLevel -Level Warning -Message "For production database changes:"
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

    # Interactive mode: Prompt for missing values
    if (-not $NonInteractive) {
        if (-not $EnvInput) {
            $EnvInput = Get-ValidatedInput "Enter environment (dev/prod)" "dev" @("dev", "prod")
        }
        # 0. Docker operations (destructive - defaults = no)
        $RecreateImageResponse = Get-ValidatedInput "Recreate Docker Image? (y/n)" "n" @("y", "n")
        $RecreateImage = $RecreateImageResponse -eq "y"
        $RecreateCacheResponse = Get-ValidatedInput "Recreate Actions Cache? (y/n)" "n" @("y", "n")
        $RecreateCache = $RecreateCacheResponse -eq "y"

        # 1. Terraform deployment
        $RunTerraformResponse = Get-ValidatedInput "Run Terraform deployment? (y/n)" "y" @("y", "n")
        $RunTerraform = $RunTerraformResponse -eq "y"
        if ($RunTerraform) {
            $AddFirewallRuleResponse = Get-ValidatedInput "  Add Firewall rule? (y/n)" "y" @("y", "n")
            $AddFirewallRule = $AddFirewallRuleResponse -eq "y"
        } else {
            $AddFirewallRule = $false
        }

        # 2. Database deployment
        $RunDatabaseResponse = Get-ValidatedInput "Run Database deployment? (y/n)" "y" @("y", "n")
        $RunDatabases = $RunDatabaseResponse -eq "y"
        if ($RunDatabases) {
            $RecreateDbResponse = Get-ValidatedInput "  Recreate Database? (y/n)" "y" @("y", "n")
            $RecreateDatabase = $RecreateDbResponse -eq "y"
            $RunDbDocsResponse = Get-ValidatedInput "  Run DB Docs? (y/n)" "y" @("y", "n")
            $RunDbDocs = $RunDbDocsResponse -eq "y"
        } else {
            $RecreateDatabase = $false
            $RunDbDocs = $false
        }

        # 3. .NET App deployment
        $RunApiResponse = Get-ValidatedInput "Run .NET App deployment? (y/n)" "y" @("y", "n")
        $RunApiDeployment = $RunApiResponse -eq "y"

        # 4. React deployment
        $RunReactResponse = Get-ValidatedInput "Run React deployment? (y/n)" "y" @("y", "n")
        $RunReactDeployment = $RunReactResponse -eq "y"

        # 5. Health Checks (conditional - only if deploying apps)
        if ($RunApiDeployment -or $RunReactDeployment) {
            $RunHealthChecksResponse = Get-ValidatedInput "Run Health Checks? (y/n)" "y" @("y", "n")
            $RunHealthChecks = $RunHealthChecksResponse -eq "y"
        } else {
            $RunHealthChecks = $false
            Write-Message -LogLevel $LogLevel -Level Info -Message "Health checks skipped (no apps being deployed)"
        }

        # Handle RecreateCache if requested
        if ($RecreateCache) {
            Write-Message -LogLevel $LogLevel -Level Warning -Message "Clearing actions cache..."
            $ActCachePath = Join-Path $env:USERPROFILE ".cache\act"
            if (Test-Path $ActCachePath) {
                Remove-Item -Path $ActCachePath -Recurse -Force
                Write-Message -LogLevel $LogLevel -Level Success -Message "Actions cache cleared"
            } else {
                Write-Message -LogLevel $LogLevel -Level Info -Message "No existing cache to clear"
            }
        }
    }
    else {
        # Non-interactive mode: Validate all required parameters are provided
        if (-not $EnvInput) {
            Show-InvalidParamError "NonInteractive mode requires -Environment parameter"
            exit 20
        }
    }

    # Convert boolean switches to string format for act (invert Run* to Skip* for workflow)
    $SkipTerraformStr = ConvertTo-ActBooleanString (-not $RunTerraform)
    $SkipDatabasesStr = ConvertTo-ActBooleanString (-not $RunDatabases)
    $RecreateDbStr = ConvertTo-ActBooleanString $RecreateDatabase
    $AllowLocalFirewallStr = ConvertTo-ActBooleanString $AddFirewallRule
    $SkipDbDocsStr = ConvertTo-ActBooleanString (-not $RunDbDocs)
    $SkipApiDeploymentStr = ConvertTo-ActBooleanString (-not $RunApiDeployment)
    $SkipReactDeploymentStr = ConvertTo-ActBooleanString (-not $RunReactDeployment)
    $SkipHealthChecksStr = ConvertTo-ActBooleanString (-not $RunHealthChecks)

    # ==============================
    # INITIALIZE ACT ACTION CACHE
    # ==============================
    # Extract pre-baked actions from Docker image to host cache for offline mode
    # This enables --action-offline-mode to work correctly without network calls

    # Call the cache utility script to initialize act cache with pre-baked actions
    # The utility script handles extraction and flattening of Docker image actions
    $cacheUtilScript = Join-Path $PSScriptRoot "lib\Initialize-ActCache.ps1"
    $ActCachePath = Join-Path $env:USERPROFILE ".cache\act"

    if (Test-Path $cacheUtilScript) {
        Write-Verbose "Calling cache utility script: $cacheUtilScript"
        & $cacheUtilScript -ImageName $LocalImageFull -Verbose:($VerbosePreference -eq 'Continue')
    } else {
        Write-Warning "Cache utility script not found: $cacheUtilScript"
        Write-Warning "Act may need to clone actions from GitHub (slower, requires network)"
    }

    # ==============================
    # SHOW SUMMARY
    # ==============================

    # Show summary in verbose mode (default)
    if ($LogLevel -eq 'Verbose') {
        Show-ExecutionSummary -Environment $EnvInput -SkipTf $SkipTerraformStr -SkipDb $SkipDatabasesStr -RecreateDb $RecreateDbStr -SecretFile $secretFile -AllowFirewall $AllowLocalFirewallStr -SkipHealthChecks $SkipHealthChecksStr
    }

    # ==============================
    # BUILD ACT COMMAND
    # ==============================
    # For AI: This builds the complete act CLI command with all configuration
    # --pull=false: Use local image only (don't download from GHCR)
    # --bind: Mount workspace for state persistence (5x performance improvement)
    # --action-offline-mode: Use pre-baked actions from /github/actions (zero network calls)
    # See docs/infra/components/local-testing.md#performance-optimization for details

    $ActArgs = @(
        "workflow_dispatch",
        "--pull=false",      # Use local image, don't pull from registry
        "--bind",            # Mount workspace for persistent state
        "--action-offline-mode",  # Use baked actions, don't pull from GitHub
        "--action-cache-path", $ActCachePath,  # Path to pre-baked action cache
        "--input", "environment=$EnvInput",
        "--input", "skip-terraform=$SkipTerraformStr",
        "--input", "skip-databases=$SkipDatabasesStr",
        "--input", "recreate-database-objects=$RecreateDbStr",
        "--input", "allow-local-firewall=$AllowLocalFirewallStr",
        "--input", "skip-health-checks=$SkipHealthChecksStr",
        "--input", "skip-db-docs=$SkipDbDocsStr",
        "--input", "skip-api-deployment=$SkipApiDeploymentStr",
        "--input", "skip-react-deployment=$SkipReactDeploymentStr",
        "-W", $WorkflowFile,
        "-P", "ubuntu-latest=$LocalImageFull"  # Use custom runner image for ubuntu-latest
    )

    # Add secret file if it exists (this will include GITHUB_TOKEN from .env.act)
    # For AI: .env.act contains all secrets - see docs/infra/components/local-testing.md
    if ($secretFile) {
        $ActArgs += "--secret-file"
        $ActArgs += $secretFile

        # CRITICAL: Also pass the same file as --var-file for GitHub Variables
        # This makes values accessible via both ${{ secrets.* }} and ${{ vars.* }}
        # Fixes Docker build tag error: vars.REACT_IMAGE_NAME was empty, causing "***/:v1-hash"
        $ActArgs += "--var-file"
        $ActArgs += $secretFile
    }
    else {
        # Fallback: provide a dummy token if no secret file
        # For AI: This is only used when .env.act is missing (rare case)
        $ActArgs += "--secret"
        $ActArgs += "GITHUB_TOKEN=ghp_dummy_token_for_local_testing"
    }

    # Add verbosity flags based on LogLevel
    if ($LogLevel -eq 'Normal') {
        Write-Verbose "Adding --quiet flag to act (LogLevel=Normal)"
        $ActArgs += "--quiet"
    }
    else {
        Write-Verbose "Using default act verbosity (LogLevel=Verbose)"
    }

    # Add GitHub Actions debug secrets based on LogLevel
    # ACTIONS_STEP_DEBUG and ACTIONS_RUNNER_DEBUG control ::debug:: output visibility
    if ($LogLevel -eq 'Verbose') {
        Write-Verbose "Enabling GitHub Actions debug logging (ACTIONS_STEP_DEBUG, ACTIONS_RUNNER_DEBUG)"
        $ActArgs += "--secret"
        $ActArgs += "ACTIONS_STEP_DEBUG=true"
        $ActArgs += "--secret"
        $ActArgs += "ACTIONS_RUNNER_DEBUG=true"
    }
    else {
        Write-Verbose "GitHub Actions debug logging disabled (LogLevel=Normal)"
        $ActArgs += "--secret"
        $ActArgs += "ACTIONS_STEP_DEBUG=false"
        $ActArgs += "--secret"
        $ActArgs += "ACTIONS_RUNNER_DEBUG=false"
    }

    # ==============================
    # EXECUTE ACT
    # ==============================

    Invoke-ActExecution $ActArgs

    Write-Host ""
    Write-Message -LogLevel $LogLevel -Level Success -Message "✅ Done!"
    Write-Host ""
}
catch {
    if ($VerbosePreference -eq 'Continue') {
        Write-Host ""
        Write-Message -LogLevel $LogLevel -Level Debug -Message "Stack trace:"
        Write-Host $_.ScriptStackTrace -ForegroundColor DarkGray
    }

    # Show specific error message if it was our custom error
    if ($_.Exception.Message.Contains("Workflow execution failed") -or $_.Exception.Message.Contains("act exited")) {
        Show-ActExecutionError
    }

    if (-not $NoWait) {
        Write-Host ""
        Write-Message -LogLevel $LogLevel -Level Info -Message "Press any key to exit..."
        $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    }

    exit 1
}
finally {
    Pop-Location -ErrorAction SilentlyContinue
}

