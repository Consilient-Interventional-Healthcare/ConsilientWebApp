@echo off
REM Script to add firewall rule to Azure SQL Server for local testing

echo Adding firewall rule for local database testing...
echo.

REM Get environment from user input (default to dev)
set /p ENV_INPUT="Enter environment (dev/prod) [default: dev]: "
if "%ENV_INPUT%"=="" set ENV_INPUT=dev

REM Resource group name
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
call az login --service-principal -u %ARM_CLIENT_ID% -p %ARM_CLIENT_SECRET% --tenant %ARM_TENANT_ID% --output none
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to login to Azure
    pause
    exit /b 1
)

call az account set --subscription %AZURE_SUBSCRIPTION_ID%

echo Getting SQL Server name...
for /f "delims=" %%i in ('az sql server list --resource-group %RESOURCE_GROUP% --query "[?contains(name, '%ENV_INPUT%')].name" -o tsv') do set SQL_SERVER_NAME=%%i

if "%SQL_SERVER_NAME%"=="" (
    echo ERROR: No SQL Server found for environment '%ENV_INPUT%'
    pause
    exit /b 1
)

echo SQL Server: %SQL_SERVER_NAME%
echo.

echo Getting your public IP address...
for /f "delims=" %%i in ('curl -s https://api.ipify.org') do set MY_IP=%%i

if "%MY_IP%"=="" (
    echo ERROR: Could not retrieve your public IP address
    pause
    exit /b 1
)

echo Your public IP: %MY_IP%
echo.

echo Adding firewall rule...
call az sql server firewall-rule create ^
    --resource-group %RESOURCE_GROUP% ^
    --server %SQL_SERVER_NAME% ^
    --name "LocalTesting-%COMPUTERNAME%" ^
    --start-ip-address %MY_IP% ^
    --end-ip-address %MY_IP%

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo ✓ Firewall rule added successfully!
    echo ========================================
    echo.
    echo Rule Name: LocalTesting-%COMPUTERNAME%
    echo IP Address: %MY_IP%
    echo.
    echo You can now test database deployment locally.
    echo.
    echo To remove this rule later, run:
    echo az sql server firewall-rule delete --resource-group %RESOURCE_GROUP% --server %SQL_SERVER_NAME% --name "LocalTesting-%COMPUTERNAME%"
) else (
    echo.
    echo ERROR: Failed to create firewall rule
)

echo.
pause
