#Requires -Version 7.0

<#
.SYNOPSIS
    Generates OpenAPI documentation JSON file for AI agents.

.DESCRIPTION
    Uses Swashbuckle CLI to extract OpenAPI spec from compiled assemblies via reflection,
    then saves it to docs/openapi.json. This documentation is optimized for AI agents to
    understand the API endpoints, verbs, inputs, and outputs.

    Defaults are configured for the Consilient.Api project.

.PARAMETER ProjectFile
    Path to the API project .csproj file. Default: Auto-detect Consilient.Api.csproj

.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Debug

.PARAMETER TargetFramework
    Target framework version (e.g., net9.0, net8.0). Default: Auto-detect from project

.PARAMETER OutputDir
    Output directory for the OpenAPI JSON file. Default: docs (relative to repository root)

.PARAMETER OutputFile
    Name of generated OpenAPI file. Default: openapi.json

.PARAMETER SwashbuckleVersion
    Required Swashbuckle CLI version. Default: 6.9.0

.PARAMETER SkipBuild
    Skip building the project (use if already built).

.PARAMETER PrettyPrint
    Format JSON with indentation for readability. Default: true

.EXAMPLE
    .\Generate-OpenApiDoc.ps1
    Generate OpenAPI documentation using default settings

.EXAMPLE
    .\Generate-OpenApiDoc.ps1 -Configuration Release
    Generate documentation using Release build

.EXAMPLE
    .\Generate-OpenApiDoc.ps1 -SkipBuild -Verbose
    Skip build (already built), show detailed output
#>

[CmdletBinding()]
param(
    [string]$ProjectFile,
    [string]$Configuration = "Debug",
    [string]$TargetFramework,
    [string]$OutputDir,
    [string]$OutputFile = "openapi.json",
    [string]$SwashbuckleVersion = "6.9.0",
    [switch]$SkipBuild,
    [bool]$PrettyPrint = $true
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Repository structure:
# ConsilientWebApp/               (repository root)
#   docs/                          (output location for openapi.json)
#   src/
#     Scripts/
#       openapi-generation/        (this script location)
#     Consilient.Api/              (API project)
#     etc.

# Check PowerShell version
if ($PSVersionTable.PSVersion.Major -lt 7) {
    Write-Host ""
    Write-Host "PowerShell 7.0 or higher is required!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Current version: PowerShell $($PSVersionTable.PSVersion)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To run this script, use PowerShell 7+:" -ForegroundColor Yellow
    Write-Host "  pwsh .\Generate-OpenApiDoc.ps1" -ForegroundColor White
    Write-Host ""
    Write-Host "Install PowerShell 7:" -ForegroundColor Cyan
    Write-Host "  https://aka.ms/install-powershell" -ForegroundColor White
    Write-Host "  or via winget: winget install Microsoft.PowerShell" -ForegroundColor White
    Write-Host ""
    exit 1
}

function Get-ProjectInfo {
    param([string]$ProjectPath)

    # Default to Consilient.Api project if not specified
    if (-not $ProjectPath) {
        # Navigate from script location to find Consilient.Api
        $srcDir = Split-Path (Split-Path $scriptDir -Parent) -Parent
        $defaultProject = Join-Path $srcDir "Consilient.Api\Consilient.Api.csproj"

        if (Test-Path $defaultProject) {
            $ProjectPath = $defaultProject
            Write-Verbose "Using default project: $ProjectPath"
        } else {
            throw "Default project not found: $defaultProject. Specify -ProjectFile parameter."
        }
    }
    elseif (-not [System.IO.Path]::IsPathRooted($ProjectPath)) {
        $ProjectPath = Join-Path (Get-Location) $ProjectPath
    }

    if (-not (Test-Path $ProjectPath)) {
        throw "Project file not found: $ProjectPath"
    }

    $projectXml = [xml](Get-Content $ProjectPath)
    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($ProjectPath)

    # Extract target framework if not specified
    if (-not $script:TargetFramework) {
        $tfm = $projectXml.Project.PropertyGroup.TargetFramework
        if ($tfm) {
            $script:TargetFramework = $tfm
            Write-Verbose "Detected target framework: $TargetFramework"
        } else {
            throw "Could not detect TargetFramework from project file. Specify -TargetFramework parameter."
        }
    }

    return @{
        Path = $ProjectPath
        Name = $projectName
        DllName = "$projectName.dll"
        Directory = Split-Path $ProjectPath -Parent
    }
}

function Write-Header {
    param([hashtable]$ProjectInfo)

    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  OpenAPI Documentation Generation" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Project: $($ProjectInfo.Name)" -ForegroundColor Gray
    Write-Host "PowerShell: $($PSVersionTable.PSVersion)" -ForegroundColor DarkGray
    Write-Verbose "Configuration: $Configuration"
    Write-Verbose "Target Framework: $TargetFramework"
    Write-Host ""
}

function Test-SwashbuckleCli {
    Write-Host "Checking Swashbuckle CLI..." -ForegroundColor Yellow

    $toolList = dotnet tool list -g 2>&1 | Out-String

    if ($toolList -notmatch "swashbuckle\.aspnetcore\.cli") {
        Write-Host "Swashbuckle CLI not found!" -ForegroundColor Red
        Write-Host ""
        Write-Host "Install with:" -ForegroundColor Yellow
        Write-Host "  dotnet tool install -g Swashbuckle.AspNetCore.Cli --version $SwashbuckleVersion" -ForegroundColor White
        Write-Host ""
        throw "Required tool 'swashbuckle.aspnetcore.cli' is not installed"
    }

    if ($toolList -match "swashbuckle\.aspnetcore\.cli\s+(\d+\.\d+\.\d+)") {
        $installedVersion = $matches[1]

        if ($installedVersion -ne $SwashbuckleVersion) {
            Write-Host "Swashbuckle CLI version mismatch!" -ForegroundColor Yellow
            Write-Host "   Expected: $SwashbuckleVersion" -ForegroundColor Gray
            Write-Host "   Installed: $installedVersion" -ForegroundColor Gray
            Write-Host ""
            Write-Host "Update with:" -ForegroundColor Yellow
            Write-Host "  dotnet tool update -g Swashbuckle.AspNetCore.Cli --version $SwashbuckleVersion" -ForegroundColor White
            Write-Host ""

            $continue = Read-Host "Continue anyway? (y/n)"
            if ($continue -ne 'y') {
                throw "Version mismatch - update Swashbuckle CLI and try again"
            }
        } else {
            Write-Host "Swashbuckle CLI v$installedVersion found" -ForegroundColor Green
        }
    }
    Write-Host ""
}

function Build-Project {
    param([string]$ProjectPath)

    if ($SkipBuild) {
        Write-Host "Skipping build (as requested)..." -ForegroundColor Yellow
        Write-Host ""
        return
    }

    Write-Host "Building project ($Configuration)..." -ForegroundColor Yellow

    if ($VerbosePreference -eq 'Continue') {
        dotnet build $ProjectPath --configuration $Configuration --no-incremental
    } else {
        dotnet build $ProjectPath --configuration $Configuration --no-incremental 2>&1 | Out-Null
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

function New-OpenApiDoc {
    param([string]$DllPath, [string]$OutputPath, [string]$ProjectDir)

    Write-Host "Generating OpenAPI documentation from assemblies..." -ForegroundColor Yellow
    Write-Host "Using assembly reflection - NO application startup!" -ForegroundColor DarkGray
    Write-Host ""

    if (-not (Test-Path $DllPath)) {
        throw "DLL not found: $DllPath. Ensure project is built."
    }

    # Change to project directory so appsettings.json files can be found
    Push-Location $ProjectDir
    try {
        # Set environment for reflection
        if (-not $env:ASPNETCORE_ENVIRONMENT) {
            $env:ASPNETCORE_ENVIRONMENT = 'Development'
        }
        $env:DOTNET_ENVIRONMENT = $env:ASPNETCORE_ENVIRONMENT
        Write-Verbose "Set ASPNETCORE_ENVIRONMENT=$env:ASPNETCORE_ENVIRONMENT for assembly reflection"

        # Generate to temp file first
        $tempFile = [System.IO.Path]::GetTempFileName()

        if ($VerbosePreference -eq 'Continue') {
            swagger tofile --output $tempFile $DllPath v1
        } else {
            swagger tofile --output $tempFile $DllPath v1 2>&1 | Out-Null
        }

        if ($LASTEXITCODE -ne 0 -or -not (Test-Path $tempFile)) {
            Write-Host ""
            Write-Host "Failed to generate OpenAPI documentation!" -ForegroundColor Red
            Write-Host ""
            Write-Host "This could mean:" -ForegroundColor Yellow
            Write-Host "  - AddSwaggerGen() not configured in Program.cs/Startup.cs" -ForegroundColor Gray
            Write-Host "  - Assembly loading issues or missing dependencies" -ForegroundColor Gray
            Write-Host "  - Version mismatch between Swashbuckle packages" -ForegroundColor Gray
            Write-Host "  - Missing configuration files" -ForegroundColor Gray
            Write-Host ""
            throw "OpenAPI documentation generation failed"
        }

        # Pretty-print if requested
        if ($PrettyPrint) {
            Write-Verbose "Pretty-printing JSON..."
            $json = Get-Content $tempFile -Raw | ConvertFrom-Json
            $json | ConvertTo-Json -Depth 100 | Set-Content $OutputPath -Encoding UTF8
            Remove-Item $tempFile
        } else {
            Move-Item $tempFile $OutputPath -Force
        }

        $docSize = [math]::Round((Get-Item $OutputPath).Length / 1KB, 2)
        Write-Host "OpenAPI documentation generated! ($docSize KB)" -ForegroundColor Green
        Write-Host ""
    }
    finally {
        Pop-Location
    }
}

function Show-Summary {
    param([string]$OutputPath)

    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  Documentation Generation Complete!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Output: " -NoNewline -ForegroundColor Gray
    Write-Host $OutputPath -ForegroundColor White
    Write-Host ""
    Write-Host "This OpenAPI document contains:" -ForegroundColor Cyan
    Write-Host "  • All API endpoints and routes" -ForegroundColor Gray
    Write-Host "  • HTTP verbs (GET, POST, PUT, DELETE, etc.)" -ForegroundColor Gray
    Write-Host "  • Request/response schemas" -ForegroundColor Gray
    Write-Host "  • Data models and types" -ForegroundColor Gray
    Write-Host "  • Parameter descriptions" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Perfect for AI agents to understand your API!" -ForegroundColor Green
    Write-Host ""
}

# ==========================================
# Main Script Execution
# ==========================================

try {
    # Get project information
    $projectInfo = Get-ProjectInfo -ProjectPath $ProjectFile

    # Show header
    Write-Header -ProjectInfo $projectInfo

    # Verify tools
    Test-SwashbuckleCli

    # Build project
    Build-Project -ProjectPath $projectInfo.Path

    # Determine output path
    if (-not $OutputDir) {
        # Default: docs folder at repository root
        # Script is in src/Scripts/openapi-generation/, go up 3 levels to reach repo root
        $srcDir = Split-Path (Split-Path $scriptDir -Parent) -Parent
        $repoRoot = Split-Path $srcDir -Parent
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

    # Determine DLL path
    $dllPath = Join-Path $projectInfo.Directory "bin\$Configuration\$TargetFramework\$($projectInfo.DllName)"

    # Generate OpenAPI documentation
    New-OpenApiDoc -DllPath $dllPath -OutputPath $fullOutputPath -ProjectDir $projectInfo.Directory

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