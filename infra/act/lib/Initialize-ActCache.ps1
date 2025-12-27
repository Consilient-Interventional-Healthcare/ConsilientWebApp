<#
.SYNOPSIS
    Extracts and flattens pre-baked GitHub Actions from Docker image to act cache.

.DESCRIPTION
    Act expects actions in flat format: owner-repo@ref
    Docker image stores them hierarchically: owner/repo@ref
    This script extracts and renames to the correct format.

.PARAMETER ImageName
    Docker image containing pre-baked actions at /github/actions

.PARAMETER CachePath
    Path to act cache directory (default: ~/.cache/act)

.EXAMPLE
    .\\Initialize-ActCache.ps1 -ImageName "consilientwebapp-runner:latest"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$ImageName,

    [Parameter(Mandatory=$false)]
    [string]$CachePath = (Join-Path $env:USERPROFILE ".cache\act")
)

# PROOF OF CONCEPT: Start with just one action
# If this works, we'll extend to all 6 actions
$ExpectedActions = @(
    "azure-login@v2"  # Most frequently cloned in tests
)

# Check if all actions already exist
$AllExist = $true
foreach ($action in $ExpectedActions) {
    if (-not (Test-Path (Join-Path $CachePath $action))) {
        $AllExist = $false
        Write-Verbose "Missing: $action"
        break
    }
}

if ($AllExist) {
    Write-Host "✅ All pre-baked actions already in cache"
    return
}

Write-Host "📦 Extracting pre-baked actions from Docker image..."

# Ensure cache directory exists
if (-not (Test-Path $CachePath)) {
    New-Item -ItemType Directory -Path $CachePath -Force | Out-Null
}

# Step 1: Extract hierarchical structure from Docker image using docker cp
# This avoids permission issues with volume mounts on Windows Docker Desktop
Write-Verbose "Creating temporary container to extract actions..."

$containerId = $null
try {
    # Create a temporary container
    $containerId = & docker create $ImageName sh -c "echo init" 2>$null
    if (-not $containerId -or $LASTEXITCODE -ne 0) {
        throw "Failed to create temporary container from image $ImageName"
    }

    Write-Verbose "Created temporary container: $containerId"
    Write-Verbose "Copying /github/actions from container..."

    # Copy actions from container to host using docker cp
    & docker cp "${containerId}:/github/actions/." "$CachePath/" 2>&1 | ForEach-Object {
        Write-Verbose $_
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Docker cp had exit code $LASTEXITCODE, but continuing..."
    }

    # Clean up container
    & docker rm $containerId -f 2>$null | Out-Null
    Write-Verbose "Cleaned up temporary container"
} catch {
    Write-Warning "Error extracting actions from Docker image: $_"
    if ($containerId) {
        & docker rm $containerId -f 2>$null | Out-Null
    }
    throw
}

# Step 2: Flatten the directory structure
# Docker creates: /host-cache/owner/repo@ref/
# We need: /host-cache/owner-repo@ref/

# PROOF OF CONCEPT: Just flatten azure directory
$ownerDirs = @("azure")  # Will expand to: @("actions", "azure", "docker", "hashicorp")
$flattenedCount = 0

foreach ($owner in $ownerDirs) {
    $ownerPath = Join-Path $CachePath $owner

    if (Test-Path $ownerPath) {
        # Get all repo@ref directories
        $repos = Get-ChildItem -Path $ownerPath -Directory -ErrorAction SilentlyContinue

        foreach ($repo in $repos) {
            $flatName = "$owner-$($repo.Name)"
            $destPath = Join-Path $CachePath $flatName

            # Move to flat structure
            Write-Verbose "Flattening: $owner/$($repo.Name) → $flatName"
            Move-Item -Path $repo.FullName -Destination $destPath -Force
            $flattenedCount++
        }

        # Remove empty owner directory
        Remove-Item -Path $ownerPath -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "✅ Extracted and flattened $flattenedCount actions"

# Verify
$verifiedCount = 0
foreach ($action in $ExpectedActions) {
    if (Test-Path (Join-Path $CachePath $action)) {
        $verifiedCount++
    }
}

if ($verifiedCount -eq $ExpectedActions.Count) {
    Write-Host "✅ All $verifiedCount expected actions verified in cache"
} else {
    Write-Warning "⚠️  Only $verifiedCount of $($ExpectedActions.Count) actions verified"
}
