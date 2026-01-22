#Requires -Version 5.1
param(
    [Parameter(Mandatory = $false)]
    [ValidateSet('ConsilientDbContext', 'UsersDbContext', 'Both')]
    [string]$Context,

    [Parameter(Mandatory = $false)]
    [ValidateRange(1, 99)]
    [int]$SequenceNumber  # Override auto-discovered prefix (e.g., -SequenceNumber 3 for 03_*)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\')).Path
$SrcRoot = Join-Path $RepoRoot 'src'
$MigrationsProject = Join-Path $SrcRoot 'Consilient.Data.Migrations'

# Available contexts
$AvailableContexts = @('ConsilientDbContext', 'UsersDbContext')

# Interactive context selection if not provided
if ([string]::IsNullOrWhiteSpace($Context)) {
    Write-Host 'Select DbContext:' -ForegroundColor Cyan
    for ($i = 0; $i -lt $AvailableContexts.Count; $i++) {
        Write-Host "  [$($i + 1)] $($AvailableContexts[$i])"
    }
    Write-Host "  [3] Both"
    do {
        $Selection = Read-Host 'Enter selection (1-3)'
        $SelectionNum = $Selection -as [int]
    } while ($SelectionNum -lt 1 -or $SelectionNum -gt 3)

    if ($SelectionNum -eq 3) {
        $Context = 'Both'
    } else {
        $Context = $AvailableContexts[$SelectionNum - 1]
    }
}

# Determine which contexts to process
if ($Context -eq 'Both') {
    $ContextsToProcess = $AvailableContexts
} else {
    $ContextsToProcess = @($Context)
}

# Process each context
foreach ($Ctx in $ContextsToProcess) {
    # Convention-based derivation
    if ($Ctx -match '^(.*)DbContext$') {
        $ContextShort = $Matches[1]
    } else {
        $ContextShort = $Ctx
    }

    # Derive paths from context
    $MigrationsDir = Join-Path $MigrationsProject $ContextShort
    $OutputDir = Join-Path $SrcRoot "Databases\$($ContextShort.ToLower())_main"

    Write-Host "Generating migration script for $Ctx..." -ForegroundColor Cyan

    # Ensure output directory exists
    if (-not (Test-Path $OutputDir)) {
        New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    }

    # Find the next sequence number based on existing files (e.g., 01_*, 02_*, etc.)
    if ($SequenceNumber) {
        $Prefix = '{0:D2}' -f $SequenceNumber
    } else {
        $ExistingFiles = Get-ChildItem -Path $OutputDir -Filter '*.sql' -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -match '^\d{2}_' } |
            Sort-Object Name -Descending

        if ($ExistingFiles) {
            $LastNumber = [int]($ExistingFiles[0].Name.Substring(0, 2))
            $NextNumber = $LastNumber + 1
        } else {
            $NextNumber = 1
        }
        $Prefix = '{0:D2}' -f $NextNumber
    }

    # Get all migrations sorted by name (excluding Designer and Snapshot files)
    $AllMigrations = Get-ChildItem -Path $MigrationsDir -Filter '*.cs' -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -match '^\d{14}_.*\.cs$' -and $_.Name -notmatch '\.Designer\.cs$' } |
        Sort-Object Name -Descending

    if ($AllMigrations.Count -eq 0) {
        Write-Host "No migrations found for $Ctx" -ForegroundColor Yellow
        continue
    }

    $LatestMigration = $AllMigrations | Select-Object -First 1
    # Extract migration name from filename (e.g., "20260115155439_AddDoctorAssignmentsStaging.cs" -> "AddDoctorAssignmentsStaging")
    $MigrationName = $LatestMigration.BaseName -replace '^\d{14}_', ''
    $LatestMigrationFullName = $LatestMigration.BaseName  # e.g., "20260115155439_AddDoctorAssignmentsStaging"

    # Determine the 'from' migration (second-to-last, or '0' if this is the first migration)
    if ($AllMigrations.Count -gt 1) {
        $PreviousMigration = $AllMigrations | Select-Object -Skip 1 -First 1
        $FromMigration = $PreviousMigration.BaseName
    } else {
        # First migration - use '0' to generate from scratch
        $FromMigration = '0'
    }

    $OutputFile = Join-Path $OutputDir "${Prefix}_${MigrationName}.sql"

    Write-Host "  From: $FromMigration" -ForegroundColor DarkGray
    Write-Host "  To:   $LatestMigrationFullName" -ForegroundColor DarkGray

    # Run from src directory where local tools are configured
    Push-Location $SrcRoot
    try {
        dotnet ef migrations script $FromMigration $LatestMigrationFullName --idempotent `
            --context $Ctx `
            --project $MigrationsProject `
            --startup-project $MigrationsProject `
            --output $OutputFile

        if ($LASTEXITCODE -ne 0) {
            Write-Host "Script generation failed for $Ctx" -ForegroundColor Red
            exit 1
        }
    }
    finally {
        Pop-Location
    }

    Write-Host "Migration script saved to: $OutputFile" -ForegroundColor Green
}

Write-Host 'Script generation completed successfully.' -ForegroundColor Green
