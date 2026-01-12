<#
.SYNOPSIS
    Centralized environment and secrets loader for Consilient scripts.

.DESCRIPTION
    Loads environment variables from the appropriate secrets file based on
    the specified environment. Supports filtering by category.

.PARAMETER Environment
    Target environment: 'local' or 'dev'. Required.

.PARAMETER Categories
    Array of categories to load: 'all', 'az', 'db', 'gh', 'loki', 'act'
    Default: 'all'

.PARAMETER PassThru
    Return loaded variables as a hashtable instead of setting environment variables.

.EXAMPLE
    # Load all secrets for local environment
    . scripts/common/Load-Environment.ps1
    Import-ConsilientEnvironment -Environment local

.EXAMPLE
    # Load only Azure and Database secrets for dev
    Import-ConsilientEnvironment -Environment dev -Categories @('az', 'db')

.EXAMPLE
    # Get secrets as hashtable without setting environment variables
    $secrets = Import-ConsilientEnvironment -Environment local -PassThru
#>

# Category mappings - which env vars belong to which category
$Script:CategoryMappings = @{
    'az'   = @('ARM_CLIENT_ID', 'ARM_CLIENT_SECRET', 'ARM_TENANT_ID', 'AZURE_SUBSCRIPTION_ID',
               'AZURE_REGION', 'AZURE_RESOURCE_GROUP_NAME', 'ACR_REGISTRY_URL')
    'db'   = @('SQL_ADMIN_USERNAME', 'SQL_ADMIN_PASSWORD', 'SQL_CONNECTION_STRING')
    'gh'   = @('GITHUB_TOKEN', 'CONTAINER_REGISTRY')
    'loki' = @('LOKI_ADDR', 'LOKI_USERNAME', 'LOKI_PASSWORD')
    'act'  = @('CAE_NAME_TEMPLATE')
}

function Import-ConsilientEnvironment {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('local', 'dev')]
        [string]$Environment,

        [Parameter(Mandatory = $false)]
        [ValidateSet('all', 'az', 'db', 'gh', 'loki', 'act')]
        [string[]]$Categories = @('all'),

        [switch]$PassThru
    )

    Write-Host "Loading environment: $Environment" -ForegroundColor Cyan

    # Find secrets file - look in scripts/ folder
    $scriptRoot = $PSScriptRoot
    $scriptsDir = (Resolve-Path (Join-Path $scriptRoot "..")).Path
    $envFile = Join-Path $scriptsDir ".env.$Environment"

    # Fallback locations for backwards compatibility
    $repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).Path
    $fallbackLocations = @(
        (Join-Path $repoRoot ".env.$Environment"),
        (Join-Path $repoRoot ".env.local"),
        (Join-Path $repoRoot "infra\act\.env.act")
    )

    if (-not (Test-Path $envFile)) {
        foreach ($fallback in $fallbackLocations) {
            if (Test-Path $fallback) {
                Write-Host "Primary file not found, using fallback: $fallback" -ForegroundColor Yellow
                $envFile = $fallback
                break
            }
        }
    }

    if (-not (Test-Path $envFile)) {
        Write-Host "No secrets file found for environment: $Environment" -ForegroundColor Red
        Write-Host "Expected location: $scriptsDir\.env.$Environment" -ForegroundColor Red
        throw "Secrets file not found"
    }

    Write-Host "Loading from: $envFile" -ForegroundColor DarkGray

    # Determine which variables to load
    $filterVars = $null
    if ($Categories -notcontains 'all') {
        $filterVars = @()
        foreach ($cat in $Categories) {
            if ($Script:CategoryMappings.ContainsKey($cat)) {
                $filterVars += $Script:CategoryMappings[$cat]
            }
        }
        Write-Host "Filtering to categories: $($Categories -join ', ')" -ForegroundColor DarkGray
    }

    # Parse and load environment file
    $loadedVars = @{}
    $loadedCount = 0

    Get-Content $envFile | ForEach-Object {
        if ($_ -match '^([^#][^=]*)=(.*)$') {
            $name = $matches[1].Trim()
            $value = $matches[2].Trim()

            # Apply category filter if specified
            if ($filterVars -and $name -notin $filterVars) {
                return
            }

            $loadedVars[$name] = $value

            if (-not $PassThru) {
                [Environment]::SetEnvironmentVariable($name, $value, 'Process')
            }
            $loadedCount++
        }
    }

    Write-Host "Loaded $loadedCount environment variables for '$Environment'" -ForegroundColor Green

    if ($PassThru) {
        return $loadedVars
    }
}

# Only export when loaded as a module
if ($MyInvocation.MyCommand.ScriptBlock.Module) {
    Export-ModuleMember -Function Import-ConsilientEnvironment
}
