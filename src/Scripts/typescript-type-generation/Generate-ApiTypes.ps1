#Requires -Version 7.0

<#
.SYNOPSIS
    Generates TypeScript interfaces from C# API without running the application.

.DESCRIPTION
    Uses Swashbuckle CLI to extract OpenAPI spec from compiled assemblies via reflection,
    then uses NSwag to generate TypeScript interfaces organized into module namespaces.

    Defaults are configured for the Consilient.Api project.

.PARAMETER ProjectFile
    Path to the API project .csproj file. Default: Auto-detect Consilient.Api.csproj

.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Debug

.PARAMETER TargetFramework
    Target framework version (e.g., net9.0, net8.0). Default: Auto-detect from project

.PARAMETER OutputDir
    TypeScript output directory. Default: ../Consilient.WebApp2/src/types (relative to API project)

.PARAMETER OutputFile
    Name of generated TypeScript file. Default: api.generated.ts

.PARAMETER NSwagConfig
    NSwag configuration file name. Default: nswag.json

.PARAMETER SwashbuckleVersion
    Required Swashbuckle CLI version. Default: 6.9.0

.PARAMETER NSwagVersion
    Required NSwag.MSBuild version. Default: 14.6.3

.PARAMETER SkipBuild
    Skip building the project (use if already built).

.PARAMETER SkipOrganize
    Skip organizing interfaces into module namespaces.

.PARAMETER KeepSwaggerJson
    Keep the intermediate swagger.json file (for debugging).

.EXAMPLE
    .\generate-api-types.ps1
    Generate types using default settings for Consilient.Api

.EXAMPLE
    .\generate-api-types.ps1 -Configuration Release
    Generate types using Release build

.EXAMPLE
    .\generate-api-types.ps1 -SkipBuild -Verbose
    Skip build (already built), show detailed output
#>

[CmdletBinding()]
param(
    [string]$ProjectFile,
    [string]$Configuration = "Debug",
    [string]$TargetFramework,
    [string]$OutputDir,
    [string]$OutputFile = "api.generated.ts",
    [string]$NSwagConfig = "nswag.json",
    [string]$SwashbuckleVersion = "6.9.0",
    [string]$NSwagVersion = "14.6.3",
    [switch]$SkipBuild,
    [switch]$SkipOrganize,
    [switch]$KeepSwaggerJson
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Check PowerShell version
if ($PSVersionTable.PSVersion.Major -lt 7) {
    Write-Host ""
    Write-Host "PowerShell 7.0 or higher is required!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Current version: PowerShell $($PSVersionTable.PSVersion)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To run this script, use PowerShell 7+:" -ForegroundColor Yellow
    Write-Host "  pwsh .\generate-api-types.ps1" -ForegroundColor White
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
    Write-Host "  TypeScript API Type Generation" -ForegroundColor Cyan
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

function Test-NSwagPackage {
    Write-Host "Checking NSwag.MSBuild..." -ForegroundColor Yellow

    # Dynamically determine the runtime folder based on target framework
    $runtimeFolder = switch -Regex ($TargetFramework) {
        '^net9\.0' { "Net90" }
        '^net8\.0' { "Net80" }
        '^net7\.0' { "Net70" }
        '^net6\.0' { "Net60" }
        default { "Net90" }
    }

    $nugetOutput = dotnet nuget locals global-packages --list
    if ($nugetOutput -match "global-packages:\s*(.+)") {
        $packageRoot = $matches[1].Trim()
    } else {
        $packageRoot = Join-Path $env:USERPROFILE ".nuget\packages"
    }

    $nswagPath = Join-Path $packageRoot "nswag.msbuild\$NSwagVersion\tools\$runtimeFolder\dotnet-nswag.dll"

    if (-not (Test-Path $nswagPath)) {
        Write-Host "NSwag.MSBuild v$NSwagVersion not found!" -ForegroundColor Red
        Write-Host "   Expected: $nswagPath" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Ensure NSwag.MSBuild package is installed in your project:" -ForegroundColor Yellow
        Write-Host "  <PackageReference Include=`"NSwag.MSBuild`" Version=`"$NSwagVersion`" />" -ForegroundColor White
        Write-Host ""
        Write-Host "Then restore packages:" -ForegroundColor Yellow
        Write-Host "  dotnet restore" -ForegroundColor White
        Write-Host ""
        throw "Required package 'NSwag.MSBuild' v$NSwagVersion not found"
    }

    Write-Host "NSwag.MSBuild v$NSwagVersion found" -ForegroundColor Green
    Write-Host ""
    return $nswagPath
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

function New-OpenApiSpec {
    param([string]$DllPath, [string]$OutputPath, [string]$ProjectDir)

    Write-Host "Generating OpenAPI spec from assemblies..." -ForegroundColor Yellow
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

        if ($VerbosePreference -eq 'Continue') {
            swagger tofile --output $OutputPath $DllPath v1
        } else {
            swagger tofile --output $OutputPath $DllPath v1 2>&1 | Out-Null
        }

        if ($LASTEXITCODE -ne 0 -or -not (Test-Path $OutputPath)) {
            Write-Host ""
            Write-Host "Failed to generate OpenAPI spec!" -ForegroundColor Red
            Write-Host ""
            Write-Host "This could mean:" -ForegroundColor Yellow
            Write-Host "  - AddSwaggerGen() not configured in Program.cs/Startup.cs" -ForegroundColor Gray
            Write-Host "  - Assembly loading issues or missing dependencies" -ForegroundColor Gray
            Write-Host "  - Version mismatch between Swashbuckle packages" -ForegroundColor Gray
            Write-Host "  - Missing configuration files" -ForegroundColor Gray
            Write-Host ""
            throw "OpenAPI spec generation failed"
        }

        $specSize = [math]::Round((Get-Item $OutputPath).Length / 1KB, 2)
        Write-Host "OpenAPI spec generated! ($specSize KB)" -ForegroundColor Green
        Write-Host ""
    }
    finally {
        Pop-Location
    }
}

function New-TypeScriptTypes {
    param([string]$NSwagPath, [string]$ConfigFile, [string]$ProjectDir)

    Write-Host "Generating TypeScript from OpenAPI spec..." -ForegroundColor Yellow

    # Look for config file in project directory
    if (-not (Test-Path $ConfigFile)) {
        $projectConfigFile = Join-Path $ProjectDir $ConfigFile
        if (Test-Path $projectConfigFile) {
            $ConfigFile = $projectConfigFile
            Write-Verbose "Using NSwag config from project directory: $ConfigFile"
        } else {
            throw "NSwag config not found: $ConfigFile (also tried $projectConfigFile)"
        }
    }

    # Run NSwag from the project directory so it can find swagger.json
    Push-Location $ProjectDir
    try {
        $output = & dotnet $NSwagPath run $ConfigFile 2>&1
        $exitCode = $LASTEXITCODE

        if ($VerbosePreference -eq 'Continue' -or $exitCode -ne 0) {
            Write-Host $output
        }

        if ($exitCode -ne 0) {
            Write-Host ""
            Write-Host "TypeScript generation failed!" -ForegroundColor Red
            throw "NSwag generation failed with exit code $exitCode"
        }

        Write-Host "TypeScript interfaces generated!" -ForegroundColor Green
        Write-Host ""
    }
    finally {
        Pop-Location
    }
}

function Test-FileUnlocked {
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        return $true
    }

    try {
        $fileStream = [System.IO.File]::Open($Path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::ReadWrite, [System.IO.FileShare]::None)
        $fileStream.Close()
        return $true
    } catch [System.IO.IOException] {
        return $false
    } catch {
        return $false
    }
}

function Wait-ForFileAvailability {
    param(
        [string]$Path,
        [int]$TimeoutSeconds = 15,
        [int]$IntervalMs = 1000
    )

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    while ($stopwatch.Elapsed.TotalSeconds -lt $TimeoutSeconds) {
        if (Test-FileUnlocked -Path $Path) {
            return $true
        }
        Start-Sleep -Milliseconds $IntervalMs
    }
    return $false
}

function Invoke-NamespaceOrganization {
    param(
        [string]$TypeScriptFile,
        [string]$SwaggerFile
    )

    if ($SkipOrganize) {
        Write-Host "Skipping namespace organization (as requested)..." -ForegroundColor Yellow
        Write-Host ""
        return
    }

    Write-Host "Organizing interfaces into module namespaces..." -ForegroundColor Yellow

    $organizerScript = Join-Path $scriptDir "Organize-ApiTypes.ps1"

    if (-not (Test-Path $organizerScript)) {
        Write-Host "Organizer script not found: $organizerScript" -ForegroundColor Yellow
        Write-Host "   Skipping namespace organization." -ForegroundColor Gray
        Write-Host ""
        return
    }

    # Wait for file to be available
    if (-not (Wait-ForFileAvailability -Path $TypeScriptFile -TimeoutSeconds 20 -IntervalMs 1000)) {
        throw "Namespace organization aborted: file is locked by another process: $TypeScriptFile.`nClose any editors/watchers that may have the file open, or re-run with -SkipOrganize."
    }

    $organizerParams = @{
        InputFile = $TypeScriptFile
    }

    if (Test-Path $SwaggerFile) {
        $organizerParams.SwaggerFile = $SwaggerFile
        Write-Verbose "Passing OpenAPI spec to organizer: $SwaggerFile"
    }

    # Execute organizer
    $output = & $organizerScript @organizerParams 2>&1
    $exitCode = $LASTEXITCODE

    if ($VerbosePreference -eq 'Continue' -or $exitCode -ne 0) {
        Write-Host $output
    }

    if ($exitCode -ne 0) {
        $combinedOutput = ($output -join "`n")
        if ($combinedOutput -match "being used by another process" -or -not (Test-FileUnlocked -Path $TypeScriptFile)) {
            throw "Namespace organization failed: file locked by another process: $TypeScriptFile.`nClose editors/watchers or run the script with -SkipOrganize to skip this step."
        }

        throw "Namespace organization failed with exit code $exitCode.`nOutput: $combinedOutput"
    }

    Write-Host "Namespace organization completed." -ForegroundColor Green
    Write-Host ""
}

function Write-Summary {
    param([string]$TypeScriptFile)

    Write-Host "Verifying output..." -ForegroundColor Yellow

    if (-not (Test-Path $TypeScriptFile)) {
        throw "TypeScript file was not created: $TypeScriptFile"
    }

    $fileInfo = Get-Item $TypeScriptFile
    $sizeKB = [math]::Round($fileInfo.Length / 1KB, 2)

    Write-Host "TypeScript file ready!" -ForegroundColor Green
    Write-Host "   Location: $TypeScriptFile" -ForegroundColor Cyan
    Write-Host "   Size: $sizeKB KB" -ForegroundColor Gray
    Write-Host "   Modified: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Success! NO APPLICATION WAS RUN!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage:" -ForegroundColor Gray
    Write-Host "  import { YourNamespace } from './api.generated';" -ForegroundColor White
    Write-Host "  const request: YourNamespace.CreateRequest = { ... };" -ForegroundColor White
    Write-Host ""
}

# Main execution
try {
    # Detect project info
    $projectInfo = Get-ProjectInfo -ProjectPath $ProjectFile

    # Set default output directory if not specified
    if (-not $OutputDir) {
        $OutputDir = Join-Path (Split-Path $projectInfo.Directory -Parent) "Consilient.WebApp2\src\types"
    }
    elseif (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
        $OutputDir = Join-Path $projectInfo.Directory $OutputDir
    }

    Write-Header -ProjectInfo $projectInfo

    # Validate dependencies
    Test-SwashbuckleCli
    $nswagPath = Test-NSwagPackage

    # Build project
    Build-Project -ProjectPath $projectInfo.Path

    # Generate OpenAPI spec
    $dllPath = Join-Path $projectInfo.Directory "bin\$Configuration\$TargetFramework\$($projectInfo.DllName)"
    $swaggerJson = Join-Path $projectInfo.Directory "swagger.json"
    New-OpenApiSpec -DllPath $dllPath -OutputPath $swaggerJson -ProjectDir $projectInfo.Directory

    # Ensure output directory exists
    if (-not (Test-Path $OutputDir)) {
        Write-Verbose "Creating output directory: $OutputDir"
        New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    }

    # Generate TypeScript
    $typeScriptFile = Join-Path $OutputDir $OutputFile
    New-TypeScriptTypes -NSwagPath $nswagPath -ConfigFile $NSwagConfig -ProjectDir $projectInfo.Directory

    # Organize namespaces
    Invoke-NamespaceOrganization -TypeScriptFile $typeScriptFile -SwaggerFile $swaggerJson

    # Summary
    Write-Summary -TypeScriptFile $typeScriptFile

    # Cleanup
    if (-not $KeepSwaggerJson -and (Test-Path $swaggerJson)) {
        Remove-Item $swaggerJson -Force
        Write-Verbose "Cleaned up swagger.json"
    }

    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  Done!" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
}
catch {
    Write-Host ""
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red

    if ($VerbosePreference -eq 'Continue') {
        Write-Host ""
        Write-Host "Stack trace:" -ForegroundColor Gray
        Write-Host $_.ScriptStackTrace -ForegroundColor DarkGray
    }

    exit 1
}
