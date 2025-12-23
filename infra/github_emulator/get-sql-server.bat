@echo off
REM Script to get Azure SQL Server FQDN for database deployment

echo Retrieving Azure SQL Server information...
echo.

REM Get environment from user input (default to dev)
set /p ENV_INPUT="Enter environment (dev/prod) [default: dev]: "
if "%ENV_INPUT%"=="" set ENV_INPUT=dev

REM Resource group name (should match your Terraform configuration)
set RESOURCE_GROUP=consilient-resource-group

echo.
echo Logging in to Azure using service principal...

REM Load credentials from .secrets file
for /f "tokens=1,2 delims==" %%a in ('type .secrets ^| findstr /v "^#"') do (
    if "%%a"=="ARM_CLIENT_ID" set ARM_CLIENT_ID=%%b
    if "%%a"=="ARM_CLIENT_SECRET" set ARM_CLIENT_SECRET=%%b
    if "%%a"=="ARM_TENANT_ID" set ARM_TENANT_ID=%%b
    if "%%a"=="AZURE_SUBSCRIPTION_ID" set AZURE_SUBSCRIPTION_ID=%%b
)

REM Login with service principal
call az login --service-principal -u %ARM_CLIENT_ID% -p %ARM_CLIENT_SECRET% --tenant %ARM_TENANT_ID% --output table
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Failed to login to Azure
    pause
    exit /b 1
)

REM Set the subscription
call az account set --subscription %AZURE_SUBSCRIPTION_ID%

echo.
echo Fetching SQL Server for environment: %ENV_INPUT%
echo Resource Group: %RESOURCE_GROUP%
echo.

REM Get SQL Server FQDN
for /f "delims=" %%i in ('az sql server list --resource-group %RESOURCE_GROUP% --query "[?contains(name, '%ENV_INPUT%')].fullyQualifiedDomainName" -o tsv') do set SQL_SERVER_FQDN=%%i

if "%SQL_SERVER_FQDN%"=="" (
    echo.
    echo ERROR: No SQL Server found for environment '%ENV_INPUT%' in resource group '%RESOURCE_GROUP%'
    echo.
    echo Make sure:
    echo 1. You have run Terraform to create the infrastructure
    echo 2. The resource group name is correct
    echo 3. You are logged into the correct Azure subscription
    echo.
    pause
    exit /b 1
)

echo.
echo ========================================
echo SQL Server FQDN: %SQL_SERVER_FQDN%
echo ========================================
echo.
echo To use this for database deployment:
echo 1. Open .env.act file
echo 2. Update the AZURE_SQL_SERVER value to: %SQL_SERVER_FQDN%
echo 3. Save the file
echo.

REM Optionally update .env.act file automatically
set /p UPDATE_FILE="Would you like to update .env.act automatically? (y/n) [default: n]: "
if /i "%UPDATE_FILE%"=="y" (
    if exist .env.act (
        echo Updating .env.act file...
        powershell -Command "(Get-Content .env.act) -replace 'AZURE_SQL_SERVER=.*', 'AZURE_SQL_SERVER=%SQL_SERVER_FQDN%' | Set-Content .env.act"
        echo.
        echo ✓ .env.act file updated successfully!
    ) else (
        echo.
        echo ERROR: .env.act file not found!
    )
)

echo.
pause
