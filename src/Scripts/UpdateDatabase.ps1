param(
    [Parameter(Mandatory=$false)]
    [string]$MigrationName,

    [Parameter(Mandatory=$false)]
    [switch]$OnlyScripts
)

$ErrorActionPreference = "Stop"
$srcRoot = $PSScriptRoot

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
$contexts = @("ConsilientDbContext", "UsersDbContext")

# Path to migrations project (used for add/script commands)
$migrationsProject = Join-Path $srcRoot "Consilient.Data.Migrations"

if (-not (Test-Path $migrationsProject)) {
    Write-Host "❌ Migrations project not found at: $migrationsProject" -ForegroundColor Red
    exit 1
}

# Add migration (skipped when -OnlyScripts is provided)
if (-not $OnlyScripts) {
    foreach ($context in $contexts) {
        Write-Host "📦 Adding migration '$MigrationName' for $context..."

        # Derive short name (strip trailing "DbContext" if present)
        if ($context -match '^(.*)DbContext$') {
            $contextShort = $matches[1]
        } else {
            $contextShort = $context
        }

        # Per-convention output folder and namespace:
        # Output folder: Consilient.Data.Migrations/<ContextShort>/
        # Namespace: Consilient.Data.Migrations.<ContextShort>
        $relativeOutputDir = $contextShort
        $fullOutputDir = Join-Path $migrationsProject $relativeOutputDir

        if (-not (Test-Path $fullOutputDir)) {
            New-Item -ItemType Directory -Path $fullOutputDir | Out-Null
        }

        $namespace = "Consilient.Data.Migrations.$contextShort"

        dotnet ef migrations add $MigrationName `
            --context $context `
            --project $migrationsProject `
            --startup-project $migrationsProject `
            --output-dir $relativeOutputDir `
            --namespace $namespace `
            --verbose

        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ AddMigration for $context failed. Stopping." -ForegroundColor Red
            exit 1
        }
    }
}
else {
    Write-Host "✅ Skipped migration creation for all contexts." -ForegroundColor Green
}

Write-Host "`n📝 Creating migration scripts..." -ForegroundColor Cyan

# Ensure output scripts directory exists
$scriptsDir = Join-Path $srcRoot ".docker\Db\consilient_main"
if (-not (Test-Path $scriptsDir)) {
    New-Item -ItemType Directory -Path $scriptsDir | Out-Null
}

$failedContexts = @()

# Generate an idempotent script per context
foreach ($context in $contexts) {
    # create a friendly short name for the file (strip trailing 'DbContext' if present)
    if ($context -match '^(.*)DbContext$') {
        $contextShort = $matches[1]
    } else {
        $contextShort = $context
    }

    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $outputFile = Join-Path $scriptsDir ("migration_{0}_{1}.sql" -f $contextShort, $timestamp)

    Write-Host "🧾 Generating idempotent script for $context -> $outputFile"

    dotnet ef migrations script --idempotent `
        --context $context `
        --project $migrationsProject `
        --startup-project $migrationsProject `
        --output $outputFile `
        --verbose

    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ CreateScript for $context failed." -ForegroundColor Red
        $failedContexts += $context
        continue
    }

    Write-Host "✅ Migration script saved to: $outputFile" -ForegroundColor Green
}

if ($failedContexts.Count -gt 0) {
    Write-Host "`n⚠️ Script generation failed for the following contexts: $($failedContexts -join ', ')" -ForegroundColor Yellow
    Write-Host "Check the verbose output above or run the failing command manually to see the exception." -ForegroundColor Yellow
    exit 1
}

Write-Host "`n✅ Migration process completed successfully!" -ForegroundColor Green