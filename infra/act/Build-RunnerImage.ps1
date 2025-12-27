<#
.SYNOPSIS
    Build and verify the custom GitHub Actions runner Docker image locally.

.DESCRIPTION
    Handles all Docker image operations for the act local testing environment:
    - Checks if Docker is running
    - Verifies if custom runner image exists
    - Builds the image if missing or if rebuild is forced
    - Returns success/failure status

    This script is called by run-act.ps1 and can also be used independently
    for Docker image maintenance.

.PARAMETER Force
    Force rebuild of the Docker image even if it already exists locally.
    Useful when updating the Dockerfile or when tools need updating.
    Default: false (only rebuild if missing)

.PARAMETER Quiet
    Suppress non-error output (minimal clean runs).
    Useful for CI/CD integration or when you want less verbose output.
    Default: Verbose mode (shows all output)

.PARAMETER Info
    Test mode: show only essential information, suppress diagnostic output.
    Useful for testing workflows without verbose diagnostic messages.
    Default: false (show diagnostic output)

.PARAMETER NonInteractive
    Run without prompts (requires image to exist or Force rebuild).
    Useful for scripting/automation.
    Default: Interactive mode (prompts if image missing)

.PARAMETER Verbose
    Show detailed verbose output for debugging.
    Works with PowerShell's -Verbose parameter.

.EXAMPLE
    .\Build-RunnerImage.ps1

    Build image if missing, interactive prompts enabled.

.EXAMPLE
    .\Build-RunnerImage.ps1 -Force -Verbose

    Force rebuild with detailed output.

.EXAMPLE
    .\Build-RunnerImage.ps1 -Force -NonInteractive -Quiet

    Force rebuild in fully automated mode with minimal output.

.NOTES
    Prerequisite: Docker Desktop (Windows/Mac) or Docker Engine (Linux)

.LINK
    https://github.com/nektos/act
#>

[CmdletBinding()]
param(
    [switch]$Force,
    [switch]$Quiet,
    [switch]$Info,
    [switch]$NonInteractive
)

# ==============================
# SCRIPT INITIALIZATION
# ==============================
$ErrorActionPreference = "Stop"
$ScriptRoot = $PSScriptRoot
$RepoRoot = Resolve-Path "$ScriptRoot\..\.."

# Import shared Write-Message helper
$WriteMessagePath = Join-Path $ScriptRoot "lib\Write-Message.ps1"
if (Test-Path $WriteMessagePath) {
    . $WriteMessagePath
}
else {
    throw "Write-Message helper not found at: $WriteMessagePath"
}

# Docker image configuration
# For AI: Custom runner image with pre-installed tools (Azure CLI, sqlcmd, Terraform, etc.)
# See docs/infra/components/github-actions.md#custom-runner-image
$LocalImageName = "consilientwebapp-runner"
$LocalImageTag = "latest"
$LocalImageFull = "${LocalImageName}:${LocalImageTag}"
$DockerfilePath = Join-Path $RepoRoot ".github\workflows\runner\Dockerfile"
$DockerContextPath = Join-Path $RepoRoot ".github\workflows\runner"


# ==============================
# PREREQUISITE VALIDATION
# ==============================

function Test-DockerRunning {
    <#
    .SYNOPSIS
        Verify Docker daemon is running and accessible.
    #>
    Write-Message -Level Info -Message "  Checking Docker..."

    try {
        $output = & docker ps 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            return $false
        }
        Write-Message -Level Debug -Message "  ✅ Docker running"
        return $true
    }
    catch {
        return $false
    }
}

function Test-DockerImageExists {
    <#
    .SYNOPSIS
        Verify that the custom runner Docker image exists locally.
    #>
    param(
        [string]$ImageName
    )

    Write-Message -Level Info -Message "  Checking Docker image..."

    try {
        $images = & docker images --format "{{.Repository}}:{{.Tag}}" 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Message -Level Warning -Message "  ⚠️ Unable to list Docker images"
            return $false
        }

        if ($images -contains $ImageName) {
            Write-Message -Level Debug -Message "  ✅ Custom runner image found ($ImageName)"
            return $true
        }
        else {
            Write-Message -Level Warning -Message "  ⚠️ Custom runner image not found ($ImageName)"
            return $false
        }
    }
    catch {
        Write-Message -Level Warning -Message "  ⚠️ Error checking Docker images"
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
    Write-Message -Level Step -Message "🔨 Building custom runner image..."
    Write-Message -Level Info -Message "  Image: $ImageName"
    Write-Message -Level Info -Message "  Dockerfile: $DockerfilePath"
    Write-Host ""

    # Verify Dockerfile exists
    if (-not (Test-Path $DockerfilePath)) {
        throw "Dockerfile not found at: $DockerfilePath"
    }

    Write-Message -Level Warning -Message "This may take a few minutes (downloading base image, installing tools)..."
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
        Write-Message -Level Debug -Message "✅ Docker image built successfully: $ImageName"
        Write-Host ""

        return $true
    }
    catch {
        Write-Host ""
        Write-Message -Level Error -Message "❌ Failed to build Docker image"
        Write-Host ""
        Write-Message -Level Error -Message "Error: $($_.Exception.Message)"
        Write-Host ""
        Write-Message -Level Warning -Message "Troubleshooting:"
        Write-Message -Level Info -Message "  1. Verify Docker has enough disk space (check Docker Desktop settings)"
        Write-Message -Level Info -Message "  2. Check network connectivity (downloads base Ubuntu image)"
        Write-Message -Level Info -Message "  3. Review Dockerfile at: $DockerfilePath"
        Write-Message -Level Info -Message "  4. Try building manually:"
        Write-Host "     docker build -t $ImageName -f $DockerfilePath $ContextPath" -ForegroundColor White
        Write-Host ""

        throw $_
    }
}

# ==============================
# ERROR HANDLING
# ==============================

function Show-DockerNotRunningError {
    Write-Host ""
    Write-Message -Level Error -Message "❌ Error: Docker is not running"
    Write-Host ""
    Write-Message -Level Warning -Message "GitHub Actions workflows run in Docker containers via act."
    Write-Message -Level Warning -Message "Docker must be running before executing this script."
    Write-Host ""
    Write-Message -Level Warning -Message "Troubleshooting:"
    Write-Message -Level Info -Message "  Windows/Mac:  Start Docker Desktop"
    Write-Message -Level Info -Message "  Linux:        sudo systemctl start docker"
    Write-Host ""
    Write-Message -Level Warning -Message "Verify with:"
    Write-Host "  docker ps" -ForegroundColor White
    Write-Host ""
    Write-Message -Level Info -Message "If Docker Desktop is installed but not running:"
    Write-Message -Level Info -Message "  1. Open Docker Desktop application"
    Write-Message -Level Info -Message "  2. Wait for the whale icon to stabilize"
    Write-Message -Level Info -Message "  3. Verify with 'docker ps' in a new terminal"
    Write-Message -Level Info -Message "  4. Try this script again"
}

# ==============================
# MAIN EXECUTION
# ==============================

try {
    Write-Message -Level Info -Message "🔍 Checking Docker prerequisites..."
    Write-Host ""

    if (-not (Test-DockerRunning)) {
        Show-DockerNotRunningError
        exit 11
    }

    # Check if custom runner image exists, build if missing or if rebuild requested
    $imageExists = Test-DockerImageExists -ImageName $LocalImageFull
    $needsBuild = (-not $imageExists) -or $Force

    if ($needsBuild) {
        if ($Force) {
            Write-Message -Level Warning -Message "Force rebuild requested for custom runner image."
        }
        else {
            Write-Message -Level Warning -Message "Custom runner image not found locally."
            Write-Message -Level Warning -Message "The image needs to be built before running act."
        }
        Write-Host ""
        Write-Message -Level Info -Message "Building image: $LocalImageFull"
        Write-Message -Level Debug -Message "  From: $DockerfilePath"
        Write-Host ""

        # Optional: Prompt for confirmation in interactive mode (unless forced rebuild)
        if (-not $NonInteractive -and -not $Force) {
            $buildConfirm = Read-Host "Build the Docker image now? (y/n) [y]"
            if ([string]::IsNullOrWhiteSpace($buildConfirm)) {
                $buildConfirm = "y"
            }

            if ($buildConfirm -ne "y") {
                Write-Host ""
                Write-Message -Level Error -Message "❌ Cannot proceed without the custom runner image"
                Write-Host ""
                Write-Message -Level Warning -Message "To build manually, run:"
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
            Write-Message -Level Error -Message "Failed to build Docker image. Cannot proceed."
            exit 13
        }
    }
    Write-Message -Level Debug -Message "✅ Docker image verification complete"
}
catch {
    if ($VerbosePreference -eq 'Continue') {
        Write-Host ""
        Write-Message -Level Debug -Message "Stack trace:"
        Write-Host $_.ScriptStackTrace -ForegroundColor DarkGray
    }

    exit 1
}
finally {
    # Cleanup if needed
}
