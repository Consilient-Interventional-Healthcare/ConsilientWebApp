$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$SrcRoot = Join-Path $RepoRoot "src"
$OutputFile = Join-Path $SrcRoot ".docker\Db\consilient_main\migration_$Timestamp.sql"
$MigrationsProject = Join-Path $SrcRoot "Consilient.Data.Migrations"

dotnet ef migrations script --idempotent `
    --context ConsilientDbContext `
    --project $MigrationsProject `
    --startup-project $MigrationsProject `
    --output $OutputFile

Write-Host "✅ Migration script saved to: $OutputFile" -ForegroundColor Green
