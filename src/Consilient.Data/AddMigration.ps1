# Interactive loop to get migration name
do {
    $MigrationName = Read-Host "Enter migration name (or press Ctrl+C to exit)"
} while (-not $MigrationName)

# Define your DbContexts
$contexts = @("ConsilientDbContext")

# Loop through each context and add migration
foreach ($context in $contexts) {
    Write-Host "📦 Adding migration '$MigrationName' for $context..."
    
    $migrationsProject = "..\Consilient.Data.Migrations"
    $namespace = "Migrations"
    $outputDir = "."
    
    dotnet ef migrations add $MigrationName `
        --context $context `
        --project $migrationsProject `
        --startup-project $migrationsProject `
        --output-dir $outputDir `
        --namespace $namespace `
        --verbose
}