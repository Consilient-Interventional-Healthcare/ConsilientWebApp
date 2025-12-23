@echo off
REM Script to enable/disable public network access on Azure SQL Server

echo Toggle SQL Server Public Network Access
echo ========================================
echo.

REM Get environment from user input (default to dev)
set /p ENV_INPUT="Enter environment (dev/prod) [default: dev]: "
if "%ENV_INPUT%"=="" set ENV_INPUT=dev

echo.
echo WARNING: This will modify the SQL Server security settings!
echo Environment: %ENV_INPUT%
echo.

set /p ACTION="Enable or Disable public access? (enable/disable) [default: enable]: "
if "%ACTION%"=="" set ACTION=enable

if /i not "%ACTION%"=="enable" if /i not "%ACTION%"=="disable" (
    echo ERROR: Invalid action. Must be 'enable' or 'disable'.
    pause
    exit /b 1
)

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

if /i "%ACTION%"=="enable" (
    echo Enabling public network access...
    call az sql server update --resource-group %RESOURCE_GROUP% --name %SQL_SERVER_NAME% --enable-public-network true

    if %ERRORLEVEL% EQU 0 (
        echo.
        echo ========================================
        echo ✓ Public network access ENABLED
        echo ========================================
        echo.
        echo Now you can add firewall rules with:
        echo   add-firewall-rule.bat
        echo.
        echo IMPORTANT: Remember to disable public access when done testing!
        echo Run: toggle-sql-public-access.bat
        echo      Then choose 'disable'
    ) else (
        echo ERROR: Failed to enable public access
    )
) else (
    echo Disabling public network access...
    call az sql server update --resource-group %RESOURCE_GROUP% --name %SQL_SERVER_NAME% --enable-public-network false

    if %ERRORLEVEL% EQU 0 (
        echo.
        echo ========================================
        echo ✓ Public network access DISABLED
        echo ========================================
        echo.
        echo SQL Server is now secured with private access only.
    ) else (
        echo ERROR: Failed to disable public access
    )
)

echo.
pause
