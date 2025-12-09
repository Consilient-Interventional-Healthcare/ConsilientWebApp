<#
.SYNOPSIS
    Generates TypeScript types for Consilient.Api project.

.DESCRIPTION
    This is a project-specific wrapper that calls the generic generate-types.ps1 script
    located in src/Scripts with the appropriate parameters for Consilient.Api.
    
    Requires PowerShell 7.0 or higher.

.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Debug

.PARAMETER SkipBuild
    Skip building the project (use if already built).

.PARAMETER SkipOrganize
    Skip organizing interfaces into module namespaces.

.PARAMETER KeepSwaggerJson
    Keep the intermediate swagger.json file (for debugging).

.PARAMETER Verbose
    Show detailed output.

.EXAMPLE
    .\Generate-ApiTypes.ps1

.EXAMPLE
    .\Generate-ApiTypes.ps1 -Configuration Release

.EXAMPLE
    .\Generate-ApiTypes.ps1 -SkipBuild -Verbose
#>

#Requires -Version 7.0

[CmdletBinding()]
param(
    [string]$Configuration = "Debug",
    [switch]$SkipBuild,
    [switch]$SkipOrganize,
    [switch]$KeepSwaggerJson
)

$ErrorActionPreference = "Stop"

# Check PowerShell version (backup check)
if ($PSVersionTable.PSVersion.Major -lt 7) {
    Write-Host ""
    Write-Host "❌ PowerShell 7.0 or higher is required!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Current version: PowerShell $($PSVersionTable.PSVersion)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To run this script, use PowerShell 7+:" -ForegroundColor Yellow
    Write-Host "  pwsh .\Generate-ApiTypes.ps1" -ForegroundColor White
    Write-Host ""
    Write-Host "Install PowerShell 7:" -ForegroundColor Cyan
    Write-Host "  https://aka.ms/install-powershell" -ForegroundColor White
    Write-Host "  or via winget: winget install Microsoft.PowerShell" -ForegroundColor White
    Write-Host ""
    exit 1
}

# Paths
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$genericScriptPath = Join-Path (Split-Path $scriptDir -Parent) "Scripts\generate-types.ps1"
$projectFile = Join-Path $scriptDir "Consilient.Api.csproj"
$outputDir = Join-Path (Split-Path $scriptDir -Parent) "Consilient.WebApp2\src\types"

# Validate generic script exists
if (-not (Test-Path $genericScriptPath)) {
    Write-Host "❌ Generic script not found: $genericScriptPath" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please ensure src/Scripts/generate-types.ps1 exists." -ForegroundColor Yellow
    exit 1
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Consilient.Api TypeScript Generation" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Project: Consilient.Api" -ForegroundColor Gray
Write-Host "Output: Consilient.WebApp2/src/types/" -ForegroundColor Gray
Write-Host "PowerShell: $($PSVersionTable.PSVersion)" -ForegroundColor DarkGray
Write-Host ""

# Build parameters for generic script
$params = @{
    ProjectFile = $projectFile
    Configuration = $Configuration
    OutputDir = $outputDir
    OutputFile = "api.generated.ts"
    NSwagConfig = "nswag.json"
    TargetFramework = "net9.0"
    SwashbuckleVersion = "6.9.0"
    NSwagVersion = "14.3.0"
}

# Add switches
if ($SkipBuild) { $params.SkipBuild = $true }
if ($SkipOrganize) { $params.SkipOrganize = $true }
if ($KeepSwaggerJson) { $params.KeepSwaggerJson = $true }
if ($VerbosePreference -eq 'Continue') { $params.Verbose = $true }

try {
    # Call generic script with project-specific parameters
    & $genericScriptPath @params
    
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}
catch {
    Write-Host ""
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
