@echo off
REM === Run GitHub Actions workflow locally using act with custom image ===

REM Change directory to the repository root
cd /d "%~dp0..\.."

REM Define paths relative to the repository root
set WORKFLOW_FILE=.github\workflows\main.yml
set DOCKERFILE=infra\github_emulator\GITHUBACTIONS.dockerfile
set ACT_SECRET_FILE=infra\github_emulator\.env.act

set IMAGE_NAME=githubactions:latest

REM Timestamp for log clarity
echo [%DATE% %TIME%] Starting workflow run...

REM Check if the image exists
docker image inspect %IMAGE_NAME% >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo Docker image %IMAGE_NAME% not found. Building it now...
    REM Use the repository root as the build context (the '.' at the end)
    docker build -t %IMAGE_NAME% -f %DOCKERFILE% .
    IF %ERRORLEVEL% NEQ 0 (
        echo Failed to build Docker image. Exiting.
        pause
        exit /b 1
    )
) ELSE (
    echo Docker image %IMAGE_NAME% found.
)

REM --- Initialize default values (can be overridden by .env.act or user input) ---
set "DEFAULT_DB_SCRIPTS_PATH=src/Databases"
set "DEFAULT_LOG_VERBOSITY=normal"

REM --- Check .env.act for potential default values before prompting ---
if exist %ACT_SECRET_FILE% (
    echo Loading potential default inputs from %ACT_SECRET_FILE%...
    for /f "tokens=1* delims==" %%a in ('type "%ACT_SECRET_FILE%" ^| findstr /i /l /c:"DB_SCRIPTS_PATH_DEFAULT=" /c:"LOG_VERBOSITY_DEFAULT="') do (
        REM Only set if the value is not empty
        if /i "%%a"=="DB_SCRIPTS_PATH_DEFAULT" if not "%%b"=="" set "DEFAULT_DB_SCRIPTS_PATH=%%b"
        if /i "%%a"=="LOG_VERBOSITY_DEFAULT" if not "%%b"=="" set "DEFAULT_LOG_VERBOSITY=%%b"
    )
    if not "%DEFAULT_DB_SCRIPTS_PATH%"=="src/Databases" echo   - Default DB_SCRIPTS_PATH found: "%DEFAULT_DB_SCRIPTS_PATH%"
    if not "%DEFAULT_LOG_VERBOSITY%"=="normal" echo   - Default LOG_VERBOSITY found: "%DEFAULT_LOG_VERBOSITY%"
    echo.
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

REM --- Prompts for DATABASE WORKFLOWS (only if database deployment is NOT skipped) ---
if /i "%SKIP_DATABASES%"=="true" (
    echo.
    echo Database deployment is SKIPPED - skipping database-specific prompts
    echo.
    set DB_SCRIPTS_PATH_INPUT=%DEFAULT_DB_SCRIPTS_PATH%
    set LOG_VERBOSITY=%DEFAULT_LOG_VERBOSITY%
    set RECREATE_DATABASE_OBJECTS=false
) else (
    echo.
    echo Configuring database deployment options...
    echo.

    REM Prompt for DB_SCRIPTS_PATH
    set "DB_SCRIPTS_PATH_PROMPT=Enter root directory for database scripts (e.g., src/Databases or src/.docker/Db)"
    if not "%DEFAULT_DB_SCRIPTS_PATH%"=="" set "DB_SCRIPTS_PATH_PROMPT=%DB_SCRIPTS_PATH_PROMPT% [default: %DEFAULT_DB_SCRIPTS_PATH%]"
    set /p DB_SCRIPTS_PATH_INPUT="%DB_SCRIPTS_PATH_PROMPT%: "
    if "%DB_SCRIPTS_PATH_INPUT%"=="" set DB_SCRIPTS_PATH_INPUT=%DEFAULT_DB_SCRIPTS_PATH%
    echo Selected database scripts path: %DB_SCRIPTS_PATH_INPUT%

    REM Ask for log verbosity level
    set "LOG_VERBOSITY_PROMPT=Log verbosity level (normal/debug)"
    if not "%DEFAULT_LOG_VERBOSITY%"=="" set "LOG_VERBOSITY_PROMPT=%LOG_VERBOSITY_PROMPT% [default: %DEFAULT_LOG_VERBOSITY%]"
    set /p LOG_VERBOSITY_INPUT="%LOG_VERBOSITY_PROMPT%: "
    if "%LOG_VERBOSITY_INPUT%"=="" set LOG_VERBOSITY_INPUT=%DEFAULT_LOG_VERBOSITY%

    if /i "%LOG_VERBOSITY_INPUT%"=="debug" (
        set LOG_VERBOSITY=debug
        echo Log verbosity: DEBUG
    ) else (
        set LOG_VERBOSITY=normal
        echo Log verbosity: NORMAL
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

    REM --- Final validation for critical inputs before calling act ---
    if "%DB_SCRIPTS_PATH_INPUT%"=="" (
        echo ERROR: DB_SCRIPTS_PATH_INPUT is empty and is a required variable for database deployment.
        pause
        exit /b 1
    )
)

REM Remove the 'set DB_SCRIPTS_PATH' here, as it's not needed as a direct environment variable for 'vars'

REM Run act with workflow file and image override, forcing use of local image
echo Running act with image %IMAGE_NAME%...
echo.

REM Check if .env.act file exists for loading secrets (now relative to root)
if exist %ACT_SECRET_FILE% (
    echo Loading environment variables from %ACT_SECRET_FILE%
    act workflow_dispatch ^
        --input environment="%ENV_INPUT%" ^
        --input skip_terraform="%SKIP_TERRAFORM%" ^
        --input skip_databases="%SKIP_DATABASES%" ^
        --input recreate_database_objects="%RECREATE_DATABASE_OBJECTS%" ^
        --input log_verbosity="%LOG_VERBOSITY%" ^
        --input DB_SCRIPTS_PATH="%DB_SCRIPTS_PATH_INPUT%" ^
        -W %WORKFLOW_FILE% ^
        -P ubuntu-latest=%IMAGE_NAME% ^
        --pull=false --bind ^
        --secret-file %ACT_SECRET_FILE%
) else (
    echo WARNING: %ACT_SECRET_FILE% file not found. Database deployment may fail without Azure credentials.
    act workflow_dispatch ^
        --input environment="%ENV_INPUT%" ^
        --input skip_terraform="%SKIP_TERRAFORM%" ^
        --input skip_databases="%SKIP_DATABASES%" ^
        --input recreate_database_objects="%RECREATE_DATABASE_OBJECTS%" ^
        --input log_verbosity="%LOG_VERBOSITY%" ^
        --input DB_SCRIPTS_PATH="%DB_SCRIPTS_PATH_INPUT%" ^
        -W %WORKFLOW_FILE% ^
        -P ubuntu-latest=%IMAGE_NAME% ^
        --pull=false --bind
)

pause