<#
.SYNOPSIS
    Executes terraform plan with all required environment variables and Azure authentication.

.DESCRIPTION
    This script loads environment variables from the .env.act file, sets up Azure
    service principal authentication, and executes terraform plan in the infra/terraform directory.

    Required environment variables (from .env.act):
    - ARM_CLIENT_ID, ARM_CLIENT_SECRET, ARM_TENANT_ID (Terraform Azure provider auth)
    - AZURE_SUBSCRIPTION_ID (Azure subscription)
    - SQL_ADMIN_USERNAME, SQL_ADMIN_PASSWORD (SQL server credentials)
    - JWT_SIGNING_SECRET, OAUTH_CLIENT_SECRET (Application secrets)
    - AZURE_REGION, AZURE_RESOURCE_GROUP_NAME (Azure infrastructure)
    - CAE_* variables (Container App Environment configuration)

.PARAMETER Environment
    Target environment (dev or prod). Default: 'dev'

.PARAMETER EnvFile
    Path to the .env.act file relative to the script location. Default: '../act/.env.act'

.PARAMETER Verbose
    Enable verbose output for detailed logging

.PARAMETER FreshState
    View plan as if the Azure environment is empty (ignores current terraform.tfstate).
    This shows what would be created from scratch without affecting the actual state file.

.EXAMPLE
    .\Run-TerraformPlan.ps1 -Environment dev
    .\Run-TerraformPlan.ps1 -Environment prod -Verbose
    .\Run-TerraformPlan.ps1 -FreshState
    .\Run-TerraformPlan.ps1 -Environment dev -FreshState -Verbose

.NOTES
    Run this script from the repository root: .\infra\scripts\Run-TerraformPlan.ps1
    Use -FreshState to see what would be deployed to an empty Azure environment.
#>

param(
    [string]$Environment = 'dev',
    [string]$EnvFile = '../act/.env.act',
    [switch]$Verbose,
    [switch]$FreshState
)

# Enable verbose output if requested
if ($Verbose) {
    $VerbosePreference = 'Continue'
}

# Get the script directory and resolve paths
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $scriptDir)
$envFilePath = Join-Path $scriptDir $EnvFile
$terraformDir = Join-Path (Join-Path $repoRoot 'infra') 'terraform'

Write-Host "[*] Terraform Plan Executor" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Validate terraform is installed
Write-Host "Checking terraform installation..." -ForegroundColor Yellow
if (-not (Get-Command terraform -ErrorAction SilentlyContinue)) {
    Write-Host "[ERROR] terraform is not installed or not in PATH" -ForegroundColor Red
    exit 1
}
Write-Host "[OK] Terraform found: $(terraform version | Select-Object -First 1)" -ForegroundColor Green

# Validate paths exist
Write-Host "Validating paths..." -ForegroundColor Yellow
if (-not (Test-Path $envFilePath)) {
    Write-Host "[ERROR] Environment file not found at: $envFilePath" -ForegroundColor Red
    exit 1
}
Write-Host "[OK] Environment file found: $envFilePath" -ForegroundColor Green

if (-not (Test-Path $terraformDir)) {
    Write-Host "[ERROR] Terraform directory not found at: $terraformDir" -ForegroundColor Red
    exit 1
}
Write-Host "[OK] Terraform directory found: $terraformDir" -ForegroundColor Green
Write-Host ""

# Function to load environment variables from .env file
function Load-EnvFile {
    param([string]$Path)

    Write-Host "Loading environment variables from .env file..." -ForegroundColor Yellow
    $envVars = @{}

    Get-Content $Path | Where-Object { $_ -and -not $_.StartsWith('#') } | ForEach-Object {
        if ($_ -match '^([^=]+)=(.*)$') {
            $key = $matches[1].Trim()
            $value = $matches[2].Trim()

            # Remove surrounding quotes if present (both single and double)
            if (($value.StartsWith('"') -and $value.EndsWith('"')) -or
                ($value.StartsWith("'") -and $value.EndsWith("'"))) {
                $value = $value.Substring(1, $value.Length - 2)
            }

            $envVars[$key] = $value
            Write-Verbose "  Loaded: $key"
        }
    }

    return $envVars
}

# Load environment variables
$envVars = Load-EnvFile $envFilePath
Write-Host "[OK] Loaded $(($envVars.Keys).Count) environment variables" -ForegroundColor Green

# Validate required variables
Write-Host "Validating required variables..." -ForegroundColor Yellow
$requiredVars = @(
    'ARM_CLIENT_ID',
    'ARM_CLIENT_SECRET',
    'ARM_TENANT_ID',
    'AZURE_SUBSCRIPTION_ID',
    'SQL_ADMIN_USERNAME',
    'SQL_ADMIN_PASSWORD',
    'JWT_SIGNING_SECRET',
    'AZURE_REGION',
    'AZURE_RESOURCE_GROUP_NAME'
)

$missingVars = @()
foreach ($var in $requiredVars) {
    if (-not $envVars.ContainsKey($var) -or [string]::IsNullOrWhiteSpace($envVars[$var])) {
        $missingVars += $var
        Write-Verbose "  [X] Missing: $var"
    } else {
        Write-Verbose "  [OK] Found: $var"
    }
}

if ($missingVars.Count -gt 0) {
    Write-Host "[ERROR] Missing required environment variables:" -ForegroundColor Red
    $missingVars | ForEach-Object { Write-Host "   - $_" -ForegroundColor Red }
    exit 1
}
Write-Host "[OK] All required variables validated" -ForegroundColor Green
Write-Host ""

# Set environment variables for terraform
Write-Host "Setting environment variables..." -ForegroundColor Yellow

# ARM provider authentication
$env:ARM_CLIENT_ID = $envVars['ARM_CLIENT_ID']
$env:ARM_CLIENT_SECRET = $envVars['ARM_CLIENT_SECRET']
$env:ARM_TENANT_ID = $envVars['ARM_TENANT_ID']

# Terraform variables
$env:TF_VAR_environment = $Environment
$env:TF_VAR_subscription_id = $envVars['AZURE_SUBSCRIPTION_ID']
$env:TF_VAR_sql_admin_username = $envVars['SQL_ADMIN_USERNAME']
$env:TF_VAR_sql_admin_password = $envVars['SQL_ADMIN_PASSWORD']
$env:TF_VAR_jwt_signing_secret = $envVars['JWT_SIGNING_SECRET']
$env:TF_VAR_oauth_client_secret = if ($envVars.ContainsKey('OAUTH_CLIENT_SECRET')) { $envVars['OAUTH_CLIENT_SECRET'] } else { '' }
$env:TF_VAR_enable_local_firewall = 'false'

# Azure infrastructure variables
$env:TF_VAR_region = $envVars['AZURE_REGION']
$env:TF_VAR_resource_group_name = $envVars['AZURE_RESOURCE_GROUP_NAME']

# Container App Environment naming configuration
$env:TF_VAR_container_app_environment_name_template = if ($envVars.ContainsKey('CAE_NAME_TEMPLATE')) { $envVars['CAE_NAME_TEMPLATE'] } else { 'consilient-cae-{environment}' }

Write-Host "[OK] Environment variables set" -ForegroundColor Green
Write-Host ""

# Change to terraform directory
Write-Host "Changing to terraform directory: $terraformDir" -ForegroundColor Yellow
Push-Location $terraformDir

try {
    # Initialize terraform if needed
    Write-Host ""
    Write-Host "Running terraform init..." -ForegroundColor Yellow
    if (-not (Test-Path '.terraform')) {
        terraform init
        if ($LASTEXITCODE -ne 0) {
            Write-Host "[ERROR] terraform init failed" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "[OK] Terraform already initialized" -ForegroundColor Green
    }

    # Handle fresh state mode
    $stateBackupPath = $null
    if ($FreshState) {
        Write-Host ""
        Write-Host "Fresh State Mode Enabled" -ForegroundColor Yellow
        Write-Host "Will show plan as if Azure environment is empty" -ForegroundColor Yellow

        if (Test-Path 'terraform.tfstate') {
            $stateBackupPath = "terraform.tfstate.backup.$(Get-Date -Format 'yyyyMMdd-HHmmss')"
            Write-Host "Backing up current state to: $stateBackupPath" -ForegroundColor Yellow
            Copy-Item 'terraform.tfstate' $stateBackupPath
            Remove-Item 'terraform.tfstate'
            Write-Host "[OK] Current state backed up" -ForegroundColor Green
        }

        if (Test-Path '.terraform/terraform.tfstate') {
            Remove-Item '.terraform/terraform.tfstate' -ErrorAction SilentlyContinue
        }

        Write-Host "[OK] Fresh state initialized" -ForegroundColor Green
    }

    # Run terraform plan
    Write-Host ""
    Write-Host "Running terraform plan for environment: $Environment" -ForegroundColor Cyan
    if ($FreshState) {
        Write-Host "(Fresh/Empty state mode)" -ForegroundColor Yellow
    }
    Write-Host "================================" -ForegroundColor Cyan
    Write-Host ""

    terraform plan

    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "================================" -ForegroundColor Cyan
        Write-Host "[SUCCESS] Terraform plan completed successfully" -ForegroundColor Green
        Write-Host ""

        # Restore state if fresh state mode was used
        if ($FreshState -and $stateBackupPath -and (Test-Path $stateBackupPath)) {
            Write-Host ""
            Write-Host "Restoring original state..." -ForegroundColor Yellow
            Remove-Item 'terraform.tfstate' -ErrorAction SilentlyContinue
            Move-Item $stateBackupPath 'terraform.tfstate'
            Write-Host "[OK] Original state restored" -ForegroundColor Green
        }
    } else {
        Write-Host ""
        Write-Host "================================" -ForegroundColor Cyan
        Write-Host "[ERROR] Terraform plan failed" -ForegroundColor Red

        # Restore state if fresh state mode was used
        if ($FreshState -and $stateBackupPath -and (Test-Path $stateBackupPath)) {
            Write-Host ""
            Write-Host "Restoring original state..." -ForegroundColor Yellow
            Remove-Item 'terraform.tfstate' -ErrorAction SilentlyContinue
            Move-Item $stateBackupPath 'terraform.tfstate'
            Write-Host "[OK] Original state restored" -ForegroundColor Green
        }
        exit 1
    }
}
finally {
    Pop-Location
}
