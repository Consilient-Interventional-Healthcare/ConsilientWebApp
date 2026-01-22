#!/usr/bin/env pwsh

[CmdletBinding()]
param (
    [Parameter(Position=0, ValueFromRemainingArguments=$true)]
    [string[]]$BuildArguments
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Push-Location $PSScriptRoot
try {
    # Build the build project first
    $buildProject = Join-Path $PSScriptRoot "build" "_build.csproj"

    Write-Host "Building NUKE build project..." -ForegroundColor Cyan
    & dotnet build $buildProject --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build the build project"
        exit $LASTEXITCODE
    }

    # Run the built executable
    $configuration = "Debug"
    $outputPath = Join-Path $PSScriptRoot "build" "bin" $configuration "_build.exe"

    if (-not (Test-Path $outputPath)) {
        Write-Error "Build executable not found at: $outputPath"
        exit 1
    }

    Write-Host "Running NUKE..." -ForegroundColor Cyan
    & $outputPath $BuildArguments
    exit $LASTEXITCODE
}
finally {
    Pop-Location
}
