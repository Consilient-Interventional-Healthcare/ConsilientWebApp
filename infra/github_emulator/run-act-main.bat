@echo off
REM === Run GitHub Actions workflow locally using act with custom image ===

set WORKFLOW_FILE=.github\workflows\main.yml
set IMAGE_NAME=githubactions:latest
set DOCKERFILE=GITHUBACTIONS.dockerfile

REM Timestamp for log clarity
echo [%DATE% %TIME%] Starting workflow run...

REM Check if the image exists
docker image inspect %IMAGE_NAME% >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo Docker image %IMAGE_NAME% not found. Building it now...
    docker build -t %IMAGE_NAME% -f %DOCKERFILE% .
    IF %ERRORLEVEL% NEQ 0 (
        echo Failed to build Docker image. Exiting.
        pause
        exit /b 1
    )
) ELSE (
    echo Docker image %IMAGE_NAME% found.
)

REM Get environment from user input (default to dev)
set /p ENV_INPUT="Enter environment (dev/prod) [default: dev]: "
if "%ENV_INPUT%"=="" set ENV_INPUT=dev

REM Validate environment input
if /i not "%ENV_INPUT%"=="dev" if /i not "%ENV_INPUT%"=="prod" (
    echo ERROR: Invalid environment '%ENV_INPUT%'. Must be 'dev' or 'prod'.
    pause
    exit /b 1
)

echo Selected environment: %ENV_INPUT%

REM Ask if terraform should be skipped (default to true for local testing)
set /p SKIP_TF="Skip Terraform deployment? (y/n) [default: y]: "
if "%SKIP_TF%"=="" set SKIP_TF=y
if /i "%SKIP_TF%"=="y" (
    set SKIP_TERRAFORM=true
    echo Terraform will be SKIPPED
) else (
    set SKIP_TERRAFORM=false
    echo Terraform will be EXECUTED
)

REM Ask if database deployment should be skipped (default to false for testing database workflow)
set /p SKIP_DB="Skip Database deployment? (y/n) [default: n]: "
if "%SKIP_DB%"=="" set SKIP_DB=n
if /i "%SKIP_DB%"=="y" (
    set SKIP_DATABASES=true
    echo Database deployment will be SKIPPED
) else (
    set SKIP_DATABASES=false
    echo Database deployment will be EXECUTED
)

REM Ask if database objects should be recreated (default to false, only allowed in dev)
set /p RECREATE_DB="Recreate all database objects? (y/n) [default: n, dev only]: "
if "%RECREATE_DB%"=="" set RECREATE_DB=n
if /i "%RECREATE_DB%"=="y" (
    set RECREATE_DATABASE_OBJECTS=true
    echo Database objects will be RECREATED ^(drops all objects first^)
) else (
    set RECREATE_DATABASE_OBJECTS=false
    echo Database objects will NOT be recreated
)

REM Ask for log verbosity level
set /p LOG_VERBOSITY="Log verbosity level (normal/debug) [default: normal]: "
if "%LOG_VERBOSITY%"=="" set LOG_VERBOSITY=normal
if /i "%LOG_VERBOSITY%"=="debug" (
    echo Log verbosity: DEBUG
) else (
    set LOG_VERBOSITY=normal
    echo Log verbosity: NORMAL
)

REM Run act with workflow file and image override, forcing use of local image
echo Running act with image %IMAGE_NAME%...
echo.

REM Check if .env.act file exists for loading secrets
if exist .env.act (
    echo Loading environment variables from .env.act
    act workflow_dispatch --input environment=%ENV_INPUT% --input skip_terraform=%SKIP_TERRAFORM% --input skip_databases=%SKIP_DATABASES% --input recreate_database_objects=%RECREATE_DATABASE_OBJECTS% --input log_verbosity=%LOG_VERBOSITY% -W %WORKFLOW_FILE% -P ubuntu-latest=%IMAGE_NAME% --pull=false --bind --secret-file .env.act
) else (
    echo WARNING: .env.act file not found. Database deployment may fail without Azure credentials.
    act workflow_dispatch --input environment=%ENV_INPUT% --input skip_terraform=%SKIP_TERRAFORM% --input skip_databases=%SKIP_DATABASES% --input recreate_database_objects=%RECREATE_DATABASE_OBJECTS% --input log_verbosity=%LOG_VERBOSITY% -W %WORKFLOW_FILE% -P ubuntu-latest=%IMAGE_NAME% --pull=false --bind
)

pause