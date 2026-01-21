#Requires -Version 7.0

<#
.SYNOPSIS
    Generates GraphQL schema SDL file from EntityGraphQL configuration.

.DESCRIPTION
    Builds and runs the Consilient.SchemaExport console app to extract the GraphQL schema
    from the configured SchemaProvider<ConsilientDbContext> and saves it as SDL format.

    This documentation is useful for understanding the GraphQL API structure and
    can be used by AI agents and developers.

.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Debug

.PARAMETER OutputDir
    Output directory for the GraphQL schema file. Default: docs (relative to repository root)

.PARAMETER OutputFile
    Name of generated GraphQL schema file. Default: schema.graphql

.PARAMETER SkipBuild
    Skip building the project (use if already built).

.EXAMPLE
    .\Generate-GraphQLSchema.ps1
    Generate GraphQL schema using default settings

.EXAMPLE
    .\Generate-GraphQLSchema.ps1 -Configuration Release
    Generate schema using Release build

.EXAMPLE
    .\Generate-GraphQLSchema.ps1 -SkipBuild -Verbose
    Skip build (already built), show detailed output
#>

[CmdletBinding()]
param(
    [string]$Configuration = "Debug",
    [string]$OutputDir,
    [string]$OutputFile = "schema.graphql",
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Repository structure:
# ConsilientWebApp/               (repository root)
#   docs/                          (output location for schema.graphql)
#   src/
#     Scripts/
#       graphql-schema-generation/ (this script location)
#     Consilient.SchemaExport/     (schema export console app)
#     etc.

# Check PowerShell version
if ($PSVersionTable.PSVersion.Major -lt 7) {
    Write-Host ""
    Write-Host "PowerShell 7.0 or higher is required!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Current version: PowerShell $($PSVersionTable.PSVersion)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To run this script, use PowerShell 7+:" -ForegroundColor Yellow
    Write-Host "  pwsh .\Generate-GraphQLSchema.ps1" -ForegroundColor White
    Write-Host ""
    Write-Host "Install PowerShell 7:" -ForegroundColor Cyan
    Write-Host "  https://aka.ms/install-powershell" -ForegroundColor White
    Write-Host "  or via winget: winget install Microsoft.PowerShell" -ForegroundColor White
    Write-Host ""
    exit 1
}

function Get-ProjectInfo {
    # Navigate from script location to find Consilient.SchemaExport
    $srcDir = Split-Path (Split-Path $scriptDir -Parent) -Parent
    $projectPath = Join-Path $srcDir "Consilient.SchemaExport\Consilient.SchemaExport.csproj"

    if (-not (Test-Path $projectPath)) {
        throw "Schema export project not found: $projectPath"
    }

    return @{
        Path = $projectPath
        Name = "Consilient.SchemaExport"
        Directory = Split-Path $projectPath -Parent
        SrcDir = $srcDir
    }
}

function Write-Header {
    param([hashtable]$ProjectInfo)

    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  GraphQL Schema Generation" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Project: $($ProjectInfo.Name)" -ForegroundColor Gray
    Write-Host "PowerShell: $($PSVersionTable.PSVersion)" -ForegroundColor DarkGray
    Write-Verbose "Configuration: $Configuration"
    Write-Host ""
}

function Build-Project {
    param([string]$ProjectPath)

    if ($SkipBuild) {
        Write-Host "Skipping build (as requested)..." -ForegroundColor Yellow
        Write-Host ""
        return
    }

    Write-Host "Building schema export project ($Configuration)..." -ForegroundColor Yellow

    if ($VerbosePreference -eq 'Continue') {
        dotnet build $ProjectPath --configuration $Configuration --nologo
    } else {
        dotnet build $ProjectPath --configuration $Configuration --nologo -v q 2>&1 | Out-Null
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "Build failed!" -ForegroundColor Red
        Write-Host ""
        Write-Host "Troubleshooting:" -ForegroundColor Yellow
        Write-Host "  1. Check build errors: dotnet build $ProjectPath" -ForegroundColor Gray
        Write-Host "  2. Clean and rebuild: dotnet clean && dotnet build" -ForegroundColor Gray
        Write-Host "  3. Restore dependencies: dotnet restore" -ForegroundColor Gray
        Write-Host ""
        throw "Build failed with exit code $LASTEXITCODE"
    }

    Write-Host "Build successful!" -ForegroundColor Green
    Write-Host ""
}

function New-GraphQLSchema {
    param([string]$ProjectPath, [string]$OutputPath)

    Write-Host "Generating GraphQL schema..." -ForegroundColor Yellow
    Write-Host "Using EntityGraphQL SchemaProvider - NO application startup!" -ForegroundColor DarkGray
    Write-Host ""

    if ($VerbosePreference -eq 'Continue') {
        dotnet run --project $ProjectPath --configuration $Configuration --no-build -- $OutputPath
    } else {
        dotnet run --project $ProjectPath --configuration $Configuration --no-build -- $OutputPath 2>&1 | Out-Null
    }

    if ($LASTEXITCODE -ne 0 -or -not (Test-Path $OutputPath)) {
        Write-Host ""
        Write-Host "Failed to generate GraphQL schema!" -ForegroundColor Red
        Write-Host ""
        Write-Host "This could mean:" -ForegroundColor Yellow
        Write-Host "  - GraphQlSchemaConfigurator.ConfigureSchema() has errors" -ForegroundColor Gray
        Write-Host "  - Missing entity types or configurations" -ForegroundColor Gray
        Write-Host "  - Assembly loading issues" -ForegroundColor Gray
        Write-Host ""
        throw "GraphQL schema generation failed"
    }

    $docSize = [math]::Round((Get-Item $OutputPath).Length / 1KB, 2)
    Write-Host "GraphQL schema generated! ($docSize KB)" -ForegroundColor Green
    Write-Host ""
}

function Show-Summary {
    param([string]$OutputPath)

    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  Schema Generation Complete!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Output: " -NoNewline -ForegroundColor Gray
    Write-Host $OutputPath -ForegroundColor White
    Write-Host ""
    Write-Host "This GraphQL schema document contains:" -ForegroundColor Cyan
    Write-Host "  - All GraphQL types and fields" -ForegroundColor Gray
    Write-Host "  - Query definitions and parameters" -ForegroundColor Gray
    Write-Host "  - Enums and input types" -ForegroundColor Gray
    Write-Host "  - Scalar definitions" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Perfect for AI agents to understand your GraphQL API!" -ForegroundColor Green
    Write-Host ""
}

# ==========================================
# Main Script Execution
# ==========================================

try {
    # Get project information
    $projectInfo = Get-ProjectInfo

    # Show header
    Write-Header -ProjectInfo $projectInfo

    # Build project
    Build-Project -ProjectPath $projectInfo.Path

    # Determine output path
    if (-not $OutputDir) {
        # Default: docs folder at repository root
        # Script is in src/Scripts/graphql-schema-generation/, go up 3 levels to reach repo root
        $repoRoot = Split-Path $projectInfo.SrcDir -Parent
        $OutputDir = Join-Path $repoRoot "docs"
    }

    if (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
        $OutputDir = Join-Path (Get-Location) $OutputDir
    }

    # Ensure output directory exists
    if (-not (Test-Path $OutputDir)) {
        Write-Verbose "Creating output directory: $OutputDir"
        New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    }

    $fullOutputPath = Join-Path $OutputDir $OutputFile

    # Generate GraphQL schema
    New-GraphQLSchema -ProjectPath $projectInfo.Path -OutputPath $fullOutputPath

    # Show summary
    Show-Summary -OutputPath $fullOutputPath

    exit 0
}
catch {
    Write-Host ""
    Write-Host "ERROR: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Stack Trace:" -ForegroundColor DarkGray
    Write-Host $_.ScriptStackTrace -ForegroundColor DarkGray
    Write-Host ""
    exit 1
}
