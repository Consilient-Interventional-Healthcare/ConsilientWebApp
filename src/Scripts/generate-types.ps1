#Requires -Version 7.0

<#
.SYNOPSIS
    Generates TypeScript interfaces from C# API without running the application.

.DESCRIPTION
    Uses Swashbuckle CLI to extract OpenAPI spec from compiled assemblies via reflection,
    then uses NSwag to generate TypeScript interfaces organized into module namespaces.
    
    This is a generic, reusable script for any .NET API project.

.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Debug

.PARAMETER TargetFramework
    Target framework version (e.g., net9.0, net8.0). Default: Auto-detect from project

.PARAMETER SwashbuckleVersion
    Required Swashbuckle CLI version. Default: 6.9.0

.PARAMETER NSwagVersion
    Required NSwag.MSBuild version. Default: 14.3.0

.PARAMETER OutputDir
    TypeScript output directory (absolute or relative to API project). Default: ../src/types

.PARAMETER OutputFile
    Name of generated TypeScript file. Default: api.generated.ts

.PARAMETER NSwagConfig
    NSwag configuration file name. Default: nswag.json

.PARAMETER ProjectFile
    Path to the API project .csproj file. Default: Auto-detect first .csproj in current directory

.PARAMETER SkipBuild
    Skip building the project (use if already built).

.PARAMETER SkipOrganize
    Skip organizing interfaces into module namespaces.

.PARAMETER KeepSwaggerJson
    Keep the intermediate swagger.json file (for debugging).

.PARAMETER Verbose
    Show detailed output.

.EXAMPLE
    .\generate-types.ps1
    Generate types using default settings (auto-detect project, Debug build)

.EXAMPLE
    .\generate-types.ps1 -Configuration Release -ProjectFile "MyApi.csproj"
    Generate types from specific project using Release build

.EXAMPLE
    .\generate-types.ps1 -SkipBuild -Verbose -OutputDir "C:\output"
    Skip build, use custom output location, show detailed output

.EXAMPLE
    .\generate-types.ps1 -TargetFramework net8.0
    Generate for .NET 8 project
#>

[CmdletBinding()]
param(
    [string]$Configuration = "Debug",
    [string]$TargetFramework,
    [string]$SwashbuckleVersion = "6.9.0",
    [string]$NSwagVersion = "14.3.0",
    [string]$OutputDir = "../src/types",
    [string]$OutputFile = "api.generated.ts",
    [string]$NSwagConfig = "nswag.json",
    [string]$ProjectFile,
    [switch]$SkipBuild,
    [switch]$SkipOrganize,
    [switch]$KeepSwaggerJson
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

function Get-ProjectInfo {
    param([string]$ProjectPath)
    
    if (-not $ProjectPath) {
        # Auto-detect: Find first .csproj in current directory
        $projects = Get-ChildItem -Path $scriptDir -Filter "*.csproj" | Select-Object -First 1
        
        if (-not $projects) {
            throw "No .csproj file found in current directory. Specify -ProjectFile parameter."
        }
        
        $ProjectPath = $projects.FullName
        Write-Verbose "Auto-detected project: $ProjectPath"
    }
    elseif (-not [System.IO.Path]::IsPathRooted($ProjectPath)) {
        $ProjectPath = Join-Path $scriptDir $ProjectPath
    }
    
    if (-not (Test-Path $ProjectPath)) {
        throw "Project file not found: $ProjectPath"
    }
    
    $projectXml = [xml](Get-Content $ProjectPath)
    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($ProjectPath)
    
    # Extract target framework if not specified
    if (-not $TargetFramework) {
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
    }
}

function Write-Header {
    param([hashtable]$ProjectInfo)
    
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  TypeScript API Type Generation" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Project: $($ProjectInfo.Name)" -ForegroundColor Gray
    Write-Verbose "Configuration: $Configuration"
    Write-Verbose "Target Framework: $TargetFramework"
    Write-Verbose "Output: $OutputDir\$OutputFile"
    Write-Host ""
}

function Test-SwashbuckleCli {
    Write-Host "Checking Swashbuckle CLI..." -ForegroundColor Yellow
    
    $toolList = dotnet tool list -g 2>&1 | Out-String
    
    if ($toolList -notmatch "swashbuckle\.aspnetcore\.cli") {
        Write-Host "? Swashbuckle CLI not found!" -ForegroundColor Red
        Write-Host ""
        Write-Host "Install with:" -ForegroundColor Yellow
        Write-Host "  dotnet tool install -g Swashbuckle.AspNetCore.Cli --version $SwashbuckleVersion" -ForegroundColor White
        Write-Host ""
        throw "Required tool 'swashbuckle.aspnetcore.cli' is not installed"
    }
    
    if ($toolList -match "swashbuckle\.aspnetcore\.cli\s+(\d+\.\d+\.\d+)") {
        $installedVersion = $matches[1]
        
        if ($installedVersion -ne $SwashbuckleVersion) {
            Write-Host "??  Swashbuckle CLI version mismatch!" -ForegroundColor Yellow
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
            Write-Host "? Swashbuckle CLI v$installedVersion found" -ForegroundColor Green
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
    
    $nswagPath = "$env:USERPROFILE\.nuget\packages\nswag.msbuild\$NSwagVersion\tools\$runtimeFolder\dotnet-nswag.dll"
    
    if (-not (Test-Path $nswagPath)) {
        Write-Host "? NSwag.MSBuild v$NSwagVersion not found!" -ForegroundColor Red
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
    
    Write-Host "? NSwag.MSBuild v$NSwagVersion found" -ForegroundColor Green
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
        Write-Host "? Build failed!" -ForegroundColor Red
        Write-Host ""
        Write-Host "Troubleshooting:" -ForegroundColor Yellow
        Write-Host "  1. Check build errors: dotnet build $ProjectPath" -ForegroundColor Gray
        Write-Host "  2. Clean and rebuild: dotnet clean && dotnet build" -ForegroundColor Gray
        Write-Host "  3. Restore dependencies: dotnet restore" -ForegroundColor Gray
        Write-Host ""
        throw "Build failed with exit code $LASTEXITCODE"
    }
    
    Write-Host "? Build successful!" -ForegroundColor Green
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
        if ($VerbosePreference -eq 'Continue') {
            swagger tofile --output $OutputPath $DllPath v1
        } else {
            swagger tofile --output $OutputPath $DllPath v1 2>&1 | Out-Null
        }
        
        if ($LASTEXITCODE -ne 0 -or -not (Test-Path $OutputPath)) {
            Write-Host ""
            Write-Host "? Failed to generate OpenAPI spec!" -ForegroundColor Red
            Write-Host ""
            Write-Host "This could mean:" -ForegroundColor Yellow
            Write-Host "  • AddSwaggerGen() not configured in Program.cs/Startup.cs" -ForegroundColor Gray
            Write-Host "  • Assembly loading issues or missing dependencies" -ForegroundColor Gray
            Write-Host "  • Version mismatch between Swashbuckle packages" -ForegroundColor Gray
            Write-Host "  • Missing configuration files" -ForegroundColor Gray
            Write-Host ""
            throw "OpenAPI spec generation failed"
        }
        
        $specSize = [math]::Round((Get-Item $OutputPath).Length / 1KB, 2)
        Write-Host "? OpenAPI spec generated! ($specSize KB)" -ForegroundColor Green
        Write-Host ""
    }
    finally {
        Pop-Location
    }
}

function New-TypeScriptTypes {
    param([string]$NSwagPath, [string]$ConfigFile, [string]$ProjectDir)
    
    Write-Host "Generating TypeScript from OpenAPI spec..." -ForegroundColor Yellow
    
    # Look for config file in project directory if not found in current directory
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
            Write-Host "? TypeScript generation failed!" -ForegroundColor Red
            throw "NSwag generation failed with exit code $exitCode"
        }
        
        Write-Host "? TypeScript interfaces generated!" -ForegroundColor Green
        Write-Host ""
    }
    finally {
        Pop-Location
    }
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
    
    $organizerScript = Join-Path $scriptDir "organize-types.ps1"
    
    if (-not (Test-Path $organizerScript)) {
        Write-Host "??  Organizer script not found: $organizerScript" -ForegroundColor Yellow
        Write-Host "   Skipping namespace organization." -ForegroundColor Gray
        Write-Host ""
        return
    }
    
    $organizerParams = @{
        InputFile = $TypeScriptFile
    }
    
    if (Test-Path $SwaggerFile) {
        $organizerParams.SwaggerFile = $SwaggerFile
        Write-Verbose "Passing OpenAPI spec to organizer: $SwaggerFile"
    }
    
    & $organizerScript @organizerParams
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "??  Namespace organization failed, but TypeScript file was generated" -ForegroundColor Yellow
    }
    
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
    
    Write-Host "? TypeScript file ready!" -ForegroundColor Green
    Write-Host "   Location: $TypeScriptFile" -ForegroundColor Cyan
    Write-Host "   Size: $sizeKB KB" -ForegroundColor Gray
    Write-Host "   Modified: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "? Success! NO APPLICATION WAS RUN!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Usage:" -ForegroundColor Gray
    Write-Host "  import { YourNamespace } from './api.generated';" -ForegroundColor White
    Write-Host "  const request: YourNamespace.CreateRequest = { ... };" -ForegroundColor White
    Write-Host ""
}

# Main execution
try {
    Push-Location $scriptDir
    
    # Detect project info
    $projectInfo = Get-ProjectInfo -ProjectPath $ProjectFile
    
    Write-Header -ProjectInfo $projectInfo
    
    # Validate dependencies
    Test-SwashbuckleCli
    $nswagPath = Test-NSwagPackage
    
    # Build project
    Build-Project -ProjectPath $projectInfo.Path
    
    # Generate OpenAPI spec
    $projectDir = Split-Path $projectInfo.Path
    $dllPath = Join-Path $projectDir "bin\$Configuration\$TargetFramework\$($projectInfo.DllName)"
    $swaggerJson = Join-Path $projectDir "swagger.json"
    New-OpenApiSpec -DllPath $dllPath -OutputPath $swaggerJson -ProjectDir $projectDir
    
    # Ensure output directory exists
    if (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
        $OutputDir = Join-Path $scriptDir $OutputDir
    }
    
    if (-not (Test-Path $OutputDir)) {
        Write-Verbose "Creating output directory: $OutputDir"
        New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    }
    
    # Generate TypeScript
    $typeScriptFile = Join-Path $OutputDir $OutputFile
    New-TypeScriptTypes -NSwagPath $nswagPath -ConfigFile $NSwagConfig -ProjectDir $projectDir
    
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
    Write-Host "? Error: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($VerbosePreference -eq 'Continue') {
        Write-Host ""
        Write-Host "Stack trace:" -ForegroundColor Gray
        Write-Host $_.ScriptStackTrace -ForegroundColor DarkGray
    }
    
    exit 1
}
finally {
    Pop-Location
}
