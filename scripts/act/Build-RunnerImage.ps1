<#
.SYNOPSIS
    Build and verify the custom GitHub Actions runner Docker image locally.

.DESCRIPTION
    Handles all Docker image operations for the act local testing environment:
    - Checks if Docker is running
    - Verifies if custom runner image exists
    - Builds the image if missing OR if rebuild is forced via -Force parameter
    - Returns success/failure status

    IMAGE REBUILD BEHAVIOR:
    The script performs a smart rebuild check:
    - Rebuilds if the image doesn't exist locally (first-time setup)
    - Rebuilds if the -Force parameter is used (e.g., after Dockerfile changes)
    - Skips rebuilding if the image exists and -Force is not used (performance optimization)

    NOTE: Dockerfile changes do NOT trigger automatic rebuilds. If you modify the
    Dockerfile or want to update tools, you must use the -Force parameter to rebuild.

    This script is called by run-act.ps1 and can also be used independently
    for Docker image maintenance.

.PARAMETER Force
    Force rebuild of the Docker image even if it already exists locally.

    Use this parameter when:
    - You've modified the Dockerfile (e.g., updated tool versions)
    - You want to pull the latest base image layer
    - You need to ensure a clean build without Docker's layer cache

    Note: Without this flag, the script skips rebuild if the image already exists,
    improving performance for repeated runs.

    Default: $false (only rebuild if missing)

.PARAMETER NonInteractive
    Run without prompts (requires image to exist or Force rebuild).
    Useful for scripting/automation.
    Default: Interactive mode (prompts if image missing)

.PARAMETER LogLevel
    Control output verbosity level.
    Valid values: 'Verbose' (show all output), 'Normal' (minimal output)
    Default: 'Verbose' (shows all diagnostic output)

.EXAMPLE
    .\Build-RunnerImage.ps1

    Standard run: checks if image exists, builds if missing, skips rebuild if it already exists.
    Use this for regular local testing with act.

.EXAMPLE
    .\Build-RunnerImage.ps1 -Force

    Force rebuild even if the image already exists locally.
    Use after modifying the Dockerfile or updating tools in the image.

.EXAMPLE
    .\Build-RunnerImage.ps1 -Force -NonInteractive -LogLevel Normal

    Force rebuild in fully automated mode with minimal output.
    Useful for CI/CD pipelines or scripts that need to ensure fresh builds.

.EXAMPLE
    # Typical workflow: Modify Dockerfile, then rebuild
    # 1. Edit .github\workflows\runner\Dockerfile (e.g., add a new tool)
    # 2. Run the build script with -Force to apply changes
    .\Build-RunnerImage.ps1 -Force

    The -Force flag ensures your Dockerfile changes are applied to the image.

.NOTES
    Prerequisite: Docker Desktop (Windows/Mac) or Docker Engine (Linux)

    REBUILD STRATEGY:
    This script uses a conservative rebuild approach to balance correctness with performance:

    1. Image Missing (First-time setup)
       - Automatically builds the image
       - Downloads base Ubuntu image (may take several minutes)

    2. Image Exists, No -Force Flag
       - Skips rebuild entirely
       - Docker layer cache is used from previous builds
       - Fast repeated runs without code changes

    3. With -Force Flag
       - Always rebuilds, even if image exists
       - Recommended when: Dockerfile modified, tools updated, or clean build needed
       - Still uses Docker layer cache (unless cache is invalidated by Dockerfile changes)

    DOCKER LAYER CACHING:
    Docker's build cache will automatically skip layers that haven't changed.
    If only some lines in the Dockerfile changed, only those layers and subsequent
    layers will be rebuilt. Earlier unchanged layers reuse cached results.

    When -Force is used with unchanged Dockerfile, most layers will still use cache,
    making rebuilds faster than the initial build.

.LINK
    https://github.com/nektos/act
#>

[CmdletBinding()]
param(
    [switch]$Force,
    [ValidateSet('Normal', 'Verbose')]
    [string]$LogLevel = 'Verbose',
    [switch]$NonInteractive
)

# ==============================
# SCRIPT INITIALIZATION
# ==============================
$ErrorActionPreference = "Stop"
$ScriptRoot = $PSScriptRoot
$RepoRoot = Resolve-Path "$ScriptRoot\..\.."

# Load shared configuration
$ConfigPath = Join-Path $ScriptRoot "ActConfig.ps1"
if (Test-Path $ConfigPath) {
    . $ConfigPath
}
else {
    throw "Shared configuration not found at: $ConfigPath"
}

# Import shared Write-Message helper
$WriteMessagePath = Join-Path $ScriptRoot "Write-Message.ps1"
if (Test-Path $WriteMessagePath) {
    . $WriteMessagePath
}
else {
    throw "Write-Message helper not found at: $WriteMessagePath"
}

# Docker image configuration
# For AI: Custom runner image with pre-installed tools (Azure CLI, sqlcmd, Terraform, etc.)
# See docs/infra/components/github-actions.md#custom-runner-image
$LocalImageName = $ActDockerConfig.LocalImageName
$LocalImageTag = $ActDockerConfig.LocalImageTag
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
    Write-Message -LogLevel $LogLevel -Level Debug -Message "  Checking Docker..."

    try {
        $output = & docker ps 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            return $false
        }
        Write-Message -LogLevel $LogLevel -Level Debug -Message "  ✅ Docker running"
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

    Write-Message -LogLevel $LogLevel -Level Debug -Message "  Checking Docker image..."

    try {
        $images = & docker images --format "{{.Repository}}:{{.Tag}}" 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Message -LogLevel $LogLevel -Level Warning -Message "  ⚠️ Unable to list Docker images"
            return $false
        }

        if ($images -contains $ImageName) {
            Write-Message -LogLevel $LogLevel -Level Debug -Message "  ✅ Custom runner image found ($ImageName)"
            return $true
        }
        else {
            Write-Message -LogLevel $LogLevel -Level Warning -Message "  ⚠️ Custom runner image not found ($ImageName)"
            return $false
        }
    }
    catch {
        Write-Message -LogLevel $LogLevel -Level Warning -Message "  ⚠️ Error checking Docker images"
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
    Write-Message -LogLevel $LogLevel -Level Debug -Message "🔨 Building custom runner image..."
    Write-Message -LogLevel $LogLevel -Level Debug -Message "  Image: $ImageName"
    Write-Message -LogLevel $LogLevel -Level Debug -Message "  Dockerfile: $DockerfilePath"
    Write-Host ""

    # Verify Dockerfile exists
    if (-not (Test-Path $DockerfilePath)) {
        throw "Dockerfile not found at: $DockerfilePath"
    }

    Write-Message -LogLevel $LogLevel -Level Warning -Message "This may take a few minutes (downloading base image, installing tools)..."
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
        Write-Message -LogLevel $LogLevel -Level Debug -Message "✅ Docker image built successfully: $ImageName"
        Write-Host ""

        return $true
    }
    catch {
        Write-Host ""
        Write-Message -LogLevel $LogLevel -Level Error -Message "❌ Failed to build Docker image"
        Write-Host ""
        Write-Message -LogLevel $LogLevel -Level Error -Message "Error: $($_.Exception.Message)"
        Write-Host ""
        Write-Message -LogLevel $LogLevel -Level Warning -Message "Troubleshooting:"
        Write-Message -LogLevel $LogLevel -Level Debug -Message "  1. Verify Docker has enough disk space (check Docker Desktop settings)"
        Write-Message -LogLevel $LogLevel -Level Debug -Message "  2. Check network connectivity (downloads base Ubuntu image)"
        Write-Message -LogLevel $LogLevel -Level Debug -Message "  3. Review Dockerfile at: $DockerfilePath"
        Write-Message -LogLevel $LogLevel -Level Debug -Message "  4. Try building manually:"
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
    Write-Message -LogLevel $LogLevel -Level Error -Message "❌ Error: Docker is not running"
    Write-Host ""
    Write-Message -LogLevel $LogLevel -Level Warning -Message "GitHub Actions workflows run in Docker containers via act."
    Write-Message -LogLevel $LogLevel -Level Warning -Message "Docker must be running before executing this script."
    Write-Host ""
    Write-Message -LogLevel $LogLevel -Level Warning -Message "Troubleshooting:"
    Write-Message -LogLevel $LogLevel -Level Debug -Message "  Windows/Mac:  Start Docker Desktop"
    Write-Message -LogLevel $LogLevel -Level Debug -Message "  Linux:        sudo systemctl start docker"
    Write-Host ""
    Write-Message -LogLevel $LogLevel -Level Warning -Message "Verify with:"
    Write-Host "  docker ps" -ForegroundColor White
    Write-Host ""
    Write-Message -LogLevel $LogLevel -Level Debug -Message "If Docker Desktop is installed but not running:"
    Write-Message -LogLevel $LogLevel -Level Debug -Message "  1. Open Docker Desktop application"
    Write-Message -LogLevel $LogLevel -Level Debug -Message "  2. Wait for the whale icon to stabilize"
    Write-Message -LogLevel $LogLevel -Level Debug -Message "  3. Verify with 'docker ps' in a new terminal"
    Write-Message -LogLevel $LogLevel -Level Debug -Message "  4. Try this script again"
}

# ==============================
# MAIN EXECUTION
# ==============================

try {
    Write-Message -LogLevel $LogLevel -Level Debug -Message "🔍 Checking Docker prerequisites..."

    if (-not (Test-DockerRunning)) {
        Show-DockerNotRunningError
        exit 11
    }

    # Check if custom runner image exists, build if missing or if rebuild requested
    $imageExists = Test-DockerImageExists -ImageName $LocalImageFull
    $needsBuild = (-not $imageExists) -or $Force

    if ($needsBuild) {
        if ($Force) {
            Write-Message -LogLevel $LogLevel -Level Warning -Message "Force rebuild requested for custom runner image."
        }
        else {
            Write-Message -LogLevel $LogLevel -Level Warning -Message "Custom runner image not found locally."
            Write-Message -LogLevel $LogLevel -Level Warning -Message "The image needs to be built before running act."
        }
        Write-Host ""
        Write-Message -LogLevel $LogLevel -Level Debug -Message "Building image: $LocalImageFull"
        Write-Message -LogLevel $LogLevel -Level Debug -Message "  From: $DockerfilePath"
        Write-Host ""

        # Optional: Prompt for confirmation in interactive mode (unless forced rebuild)
        if (-not $NonInteractive -and -not $Force) {
            $buildConfirm = Read-Host "Build the Docker image now? (y/n) [y]"
            if ([string]::IsNullOrWhiteSpace($buildConfirm)) {
                $buildConfirm = "y"
            }

            if ($buildConfirm -ne "y") {
                Write-Host ""
                Write-Message -LogLevel $LogLevel -Level Error -Message "❌ Cannot proceed without the custom runner image"
                Write-Host ""
                Write-Message -LogLevel $LogLevel -Level Warning -Message "To build manually, run:"
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
            Write-Message -LogLevel $LogLevel -Level Error -Message "Failed to build Docker image. Cannot proceed."
            exit 13
        }
    }
    Write-Message -LogLevel $LogLevel -Level Debug -Message "✅ Docker image verification complete"

    # Provide helpful tip when image exists and no rebuild was requested
    if ($imageExists -and -not $Force) {
        Write-Host ""
        Write-Message -LogLevel $LogLevel -Level Debug -Message "💡 Tip: If you've modified the Dockerfile and want to rebuild, use:"
        Write-Message -LogLevel $LogLevel -Level Debug -Message "  .\Build-RunnerImage.ps1 -Force"
    }
}
catch {
    if ($VerbosePreference -eq 'Continue') {
        Write-Host ""
        Write-Message -LogLevel $LogLevel -Level Debug -Message "Stack trace:"
        Write-Host $_.ScriptStackTrace -ForegroundColor DarkGray
    }

    exit 1
}
finally {
    # Cleanup if needed
}
