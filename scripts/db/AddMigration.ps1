param(
    [Parameter(Mandatory=$false)]
    [string]$MigrationName
)

# Interactive loop to get migration name if not provided as parameter
if ([string]::IsNullOrWhiteSpace($MigrationName)) {
    do {
        $MigrationName = Read-Host "Enter migration name (or press Ctrl+C to exit)"
    } while (-not $MigrationName)
}

$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$SrcRoot = Join-Path $RepoRoot "src"
$MigrationsProject = Join-Path $SrcRoot "Consilient.Data.Migrations"

# Define your DbContexts
$Contexts = @("ConsilientDbContext")

# Loop through each context and add migration
foreach ($Context in $Contexts) {
    Write-Host "📦 Adding migration '$MigrationName' for $Context..."

    $Namespace = "Migrations"
    $OutputDir = "."

    dotnet ef migrations add $MigrationName `
        --context $Context `
        --project $MigrationsProject `
        --startup-project $MigrationsProject `
        --output-dir $OutputDir `
        --namespace $Namespace `
        --verbose
}
