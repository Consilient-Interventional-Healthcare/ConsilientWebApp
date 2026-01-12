param(
    [Parameter(Mandatory=$false)]
    [string]$MigrationName,

    [Parameter(Mandatory=$false)]
    [switch]$OnlyScripts
)

$ErrorActionPreference = "Stop"
$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$SrcRoot = Join-Path $RepoRoot "src"

Write-Host "🚀 Starting migration process..." -ForegroundColor Cyan

# If not only generating scripts, interactively get migration name when missing
if (-not $OnlyScripts) {
    if ([string]::IsNullOrWhiteSpace($MigrationName)) {
        do {
            $MigrationName = Read-Host "Enter migration name (or press Ctrl+C to exit)"
        } while (-not $MigrationName)
    }
}
else {
    Write-Host "📄 OnlyScripts flag set — skipping creation of new migrations and only generating scripts." -ForegroundColor Cyan
}

# Define your DbContexts
$Contexts = @("ConsilientDbContext", "UsersDbContext")

# Path to migrations project (used for add/script commands)
$MigrationsProject = Join-Path $SrcRoot "Consilient.Data.Migrations"

if (-not (Test-Path $MigrationsProject)) {
    Write-Host "❌ Migrations project not found at: $MigrationsProject" -ForegroundColor Red
    exit 1
}

# Add migration (skipped when -OnlyScripts is provided)
if (-not $OnlyScripts) {
    foreach ($Context in $Contexts) {
        Write-Host "📦 Adding migration '$MigrationName' for $Context..."

        # Derive short name (strip trailing "DbContext" if present)
        if ($Context -match '^(.*)DbContext$') {
            $ContextShort = $matches[1]
        } else {
            $ContextShort = $Context
        }

        # Per-convention output folder and namespace:
        # Output folder: Consilient.Data.Migrations/<ContextShort>/
        # Namespace: Consilient.Data.Migrations.<ContextShort>
        $RelativeOutputDir = $ContextShort
        $FullOutputDir = Join-Path $MigrationsProject $RelativeOutputDir

        if (-not (Test-Path $FullOutputDir)) {
            New-Item -ItemType Directory -Path $FullOutputDir | Out-Null
        }

        $Namespace = "Consilient.Data.Migrations.$ContextShort"

        dotnet ef migrations add $MigrationName `
            --context $Context `
            --project $MigrationsProject `
            --startup-project $MigrationsProject `
            --output-dir $RelativeOutputDir `
            --namespace $Namespace `
            --verbose

        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ AddMigration for $Context failed. Stopping." -ForegroundColor Red
            exit 1
        }
    }
}
else {
    Write-Host "✅ Skipped migration creation for all contexts." -ForegroundColor Green
}

Write-Host "`n📝 Creating migration scripts..." -ForegroundColor Cyan

# Ensure output scripts directory exists
$ScriptsDir = Join-Path $SrcRoot ".docker\Db\consilient_main"
if (-not (Test-Path $ScriptsDir)) {
    New-Item -ItemType Directory -Path $ScriptsDir | Out-Null
}

$FailedContexts = @()

# Generate an idempotent script per context
foreach ($Context in $Contexts) {
    # create a friendly short name for the file (strip trailing 'DbContext' if present)
    if ($Context -match '^(.*)DbContext$') {
        $ContextShort = $matches[1]
    } else {
        $ContextShort = $Context
    }

    $Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $OutputFile = Join-Path $ScriptsDir ("migration_{0}_{1}.sql" -f $ContextShort, $Timestamp)

    Write-Host "🧾 Generating idempotent script for $Context -> $OutputFile"

    dotnet ef migrations script --idempotent `
        --context $Context `
        --project $MigrationsProject `
        --startup-project $MigrationsProject `
        --output $OutputFile `
        --verbose

    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ CreateScript for $Context failed." -ForegroundColor Red
        $FailedContexts += $Context
        continue
    }

    Write-Host "✅ Migration script saved to: $OutputFile" -ForegroundColor Green
}

if ($FailedContexts.Count -gt 0) {
    Write-Host "`n⚠️ Script generation failed for the following contexts: $($FailedContexts -join ', ')" -ForegroundColor Yellow
    Write-Host "Check the verbose output above or run the failing command manually to see the exception." -ForegroundColor Yellow
    exit 1
}

Write-Host "`n✅ Migration process completed successfully!" -ForegroundColor Green
