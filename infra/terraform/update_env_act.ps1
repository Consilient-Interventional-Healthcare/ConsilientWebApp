param(
    [string]$EnvActFile,
    [string]$AcrRegistry
)

$ErrorActionPreference = "Stop"

Write-Host "Updating $EnvActFile with ACR credentials..."

# Read the file contents
$lines = @()
$clientIdRead = $false
$clientSecretRead = $false
$registryRead = $false

# Read client ID and secret from files
$clientId = Get-Content "acr_client_id.txt" -Raw | ForEach-Object { $_.Trim() }
$clientSecret = Get-Content "acr_client_secret.txt" -Raw | ForEach-Object { $_.Trim() }

# Read and update the env file
$content = Get-Content $EnvActFile -Raw

# Update or add ACR_REGISTRY
if ($content -match '(?m)^ACR_REGISTRY=.*$') {
    $content = $content -replace '(?m)^ACR_REGISTRY=.*$', "ACR_REGISTRY=$AcrRegistry"
} else {
    $content += "`nACR_REGISTRY=$AcrRegistry"
}

# Update or add ACR_CICD_CLIENT_ID
if ($content -match '(?m)^ACR_CICD_CLIENT_ID=.*$') {
    $content = $content -replace '(?m)^ACR_CICD_CLIENT_ID=.*$', "ACR_CICD_CLIENT_ID=$clientId"
} else {
    $content += "`nACR_CICD_CLIENT_ID=$clientId"
}

# Update or add ACR_CICD_CLIENT_SECRET
if ($content -match '(?m)^ACR_CICD_CLIENT_SECRET=.*$') {
    $content = $content -replace '(?m)^ACR_CICD_CLIENT_SECRET=.*$', "ACR_CICD_CLIENT_SECRET=$clientSecret"
} else {
    $content += "`nACR_CICD_CLIENT_SECRET=$clientSecret"
}

# Write back to file
Set-Content -Path $EnvActFile -Value $content -Encoding UTF8

Write-Host ".env.act updated successfully!"
