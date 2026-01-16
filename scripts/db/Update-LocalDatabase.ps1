#Requires -Version 5.1
param(
    [Parameter(Mandatory = $false)]
    [ValidateSet('ConsilientDbContext', 'UsersDbContext', 'Both')]
    [string]$Context = 'Both'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\')).Path
$SrcRoot = Join-Path $RepoRoot 'src'
$MigrationsProject = Join-Path $SrcRoot 'Consilient.Data.Migrations'

# Docker database connection settings
$DbServer = 'localhost,1434'
$DbUser = 'sa'
$DbPassword = 'YourStrong!Passw0rd'

# Available contexts (both use consilient_main database)
$AvailableContexts = @('ConsilientDbContext', 'UsersDbContext')

# Determine which contexts to process
if ($Context -eq 'Both') {
    $ContextsToProcess = $AvailableContexts
} else {
    $ContextsToProcess = @($Context)
}

# Build connection string (both contexts use consilient_main)
$ConnectionString = "Server=$DbServer;Database=consilient_main;User Id=$DbUser;Password=$DbPassword;TrustServerCertificate=True;"

# Process each context
foreach ($Ctx in $ContextsToProcess) {
    Write-Host "Applying migrations for $Ctx..." -ForegroundColor Cyan

    Push-Location $SrcRoot
    try {
        dotnet ef database update `
            --context $Ctx `
            --project $MigrationsProject `
            --startup-project $MigrationsProject `
            --connection $ConnectionString `
            --verbose

        if ($LASTEXITCODE -ne 0) {
            Write-Host "Migration failed for $Ctx" -ForegroundColor Red
            exit 1
        }
    }
    finally {
        Pop-Location
    }

    Write-Host "Migrations applied successfully for $Ctx." -ForegroundColor Green
}

Write-Host 'Database update completed.' -ForegroundColor Green
