$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$outputFile = "..\.docker\Db\consilient_main\migration_$timestamp.sql"
dotnet ef migrations script --idempotent `
    --context ConsilientDbContext `
    --project ..\Consilient.Data.Migrations `
    --startup-project . `
    --output $outputFile

Write-Host "✅ Migration script saved to: $outputFile" -ForegroundColor Green