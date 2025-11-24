param(
    [Parameter(Mandatory=$false)]
    [string]$MigrationName
)

$ErrorActionPreference = "Stop"
$srcRoot = $PSScriptRoot

Write-Host "🚀 Starting migration process..." -ForegroundColor Cyan

# Interactive loop to get migration name if not provided as parameter
if ([string]::IsNullOrWhiteSpace($MigrationName)) {
    do {
        $MigrationName = Read-Host "Enter migration name (or press Ctrl+C to exit)"
    } while (-not $MigrationName)
}

# Define your DbContexts
$contexts = @("ConsilientDbContext", "UsersDbContext")

# Path to migrations project
$migrationsProject = Join-Path $srcRoot "Consilient.Data.Migrations"

if (-not (Test-Path $migrationsProject)) {
    Write-Host "❌ Migrations project not found at: $migrationsProject" -ForegroundColor Red
    exit 1
}

# Add migration
foreach ($context in $contexts) {
    Write-Host "📦 Adding migration '$MigrationName' for $context..."
    
    # Derive short name (strip trailing "DbContext" if present)
    if ($context -match '^(.*)DbContext$') {
        $contextShort = $matches[1]
    } else {
        $contextShort = $context
    }

    # Output folder per convention: Consilient.Data.Migrations/<ContextShort>/
    $relativeOutputDir = $contextShort
    $fullOutputDir = Join-Path $migrationsProject $relativeOutputDir

    if (-not (Test-Path $fullOutputDir)) {
        New-Item -ItemType Directory -Path $fullOutputDir | Out-Null
    }

    # Namespace per convention: Consilient.Data.Migrations.<ContextShort>
    $namespace = "Consilient.Data.Migrations.$contextShort"

    dotnet ef migrations add $MigrationName `
        --context $context `
        --project $migrationsProject `
        --startup-project $migrationsProject `
        --output-dir $relativeOutputDir `
        --namespace $namespace `
        --verbose
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ AddMigration failed. Stopping." -ForegroundColor Red
        exit 1
    }
}

Write-Host "`n📝 Creating migration script..." -ForegroundColor Cyan

# Create script
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$outputFile = Join-Path $srcRoot ".docker\Db\consilient_main\migration_$timestamp.sql"
$migrationsProject = Join-Path $srcRoot "Consilient.Data.Migrations"

dotnet ef migrations script --idempotent `
    --context ConsilientDbContext `
    --project $migrationsProject `
    --startup-project $migrationsProject `
    --output $outputFile

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ CreateScript failed." -ForegroundColor Red
    exit 1
}

Write-Host "✅ Migration script saved to: $outputFile" -ForegroundColor Green
Write-Host "`n✅ Migration process completed successfully!" -ForegroundColor Green