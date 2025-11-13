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
$contexts = @("ConsilientDbContext")

# Add migration
foreach ($context in $contexts) {
    Write-Host "📦 Adding migration '$MigrationName' for $context..."
    
    $migrationsProject = Join-Path $srcRoot "Consilient.Data.Migrations"
    
    dotnet ef migrations add $MigrationName `
        --context $context `
        --project $migrationsProject `
        --startup-project $migrationsProject `
        --output-dir "." `
        --namespace "Migrations" `
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