param(
    [string]$AcrName
)

$ErrorActionPreference = "Stop"

$AppName = "$($AcrName)-cicd"

Write-Host "Creating Azure AD Application and Service Principal for ACR..."

# Check if app already exists
$existingApp = az ad app list --filter "displayName eq '$AppName'" --query "[0].id" -o tsv 2>$null

if ($existingApp) {
    Write-Host "Azure AD Application already exists: $AppName"
    $AppId = $existingApp
} else {
    Write-Host "Creating new Azure AD Application: $AppName"
    $AppId = az ad app create --display-name "$AppName" --query id -o tsv
}

Write-Host "Application ID: $AppId"

# Create or get service principal
$spObjId = az ad sp show --id "$AppId" --query id -o tsv 2>$null

if (-not $spObjId) {
    Write-Host "Creating Service Principal..."
    az ad sp create --id "$AppId" > $null
    $spObjId = az ad sp show --id "$AppId" --query id -o tsv
}

Write-Host "Service Principal Object ID: $spObjId"

# Create or rotate client secret
Write-Host "Creating client secret..."
$clientSecret = az ad app credential create --id "$AppId" --display-name "github-actions" --query password -o tsv
$clientId = az ad app show --id "$AppId" --query appId -o tsv

Write-Host "Client ID: $clientId"

# Save values to files for Terraform to read
$spObjId | Out-File -FilePath "acr_sp_object_id.txt" -Encoding UTF8 -NoNewline
$clientId | Out-File -FilePath "acr_client_id.txt" -Encoding UTF8 -NoNewline
$clientSecret | Out-File -FilePath "acr_client_secret.txt" -Encoding UTF8 -NoNewline

Write-Host "Service Principal created successfully!"
