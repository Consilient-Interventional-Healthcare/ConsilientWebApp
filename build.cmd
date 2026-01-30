@echo off
REM ============================================================================
REM CONSILIENT BUILD TOOL
REM ============================================================================
REM See build/BUILD.md for full documentation

setlocal

REM Handle help flags before anything else
if "%~1"=="--help" goto :ShowHelp
if "%~1"=="-h" goto :ShowHelp
if "%~1"=="/?" goto :ShowHelp

pushd "%~dp0"

echo Building NUKE build project...
dotnet build build\_build.csproj --verbosity quiet
if errorlevel 1 (
    echo Failed to build the build project
    popd
    exit /b %ERRORLEVEL%
)

REM If no arguments, launch interactive menu
if "%~1"=="" (
    build\bin\Debug\_build.exe InteractiveMenu
) else (
    build\bin\Debug\_build.exe %*
)

popd
exit /b %ERRORLEVEL%

:ShowHelp
echo.
echo CONSILIENT BUILD TOOL
echo =====================
echo.
echo USAGE:
echo   build.cmd                     Launch interactive menu
echo   build.cmd [target] [options]  Run specific target
echo   build.cmd --help              Show this help
echo.
echo TARGETS:
echo   GenerateAllTypes       Generate all code types (OpenAPI, GraphQL, TypeScript)
echo   AddMigration           Add new EF Core migration
echo   UpdateLocalDatabase    Apply pending migrations
echo   SquashMigrations       Consolidate migrations (requires --force)
echo   RebuildDatabase        Rebuild local database (requires --force)
echo   GenerateDatabaseDocs   Generate database documentation
echo   DockerUp               Start Docker services
echo   DockerDown             Stop Docker services
echo   DockerNuclearReset     Complete Docker cleanup (requires --force)
echo.
echo PARAMETERS:
echo   --force                Skip confirmations for destructive operations
echo   --db-context NAME      Target: ConsilientDbContext, UsersDbContext, or Both
echo   --migration-name NAME  Migration name (required for AddMigration)
echo   --backup               Create backup before rebuild
echo   --db-auto-start        Auto-start database container
echo.
echo EXAMPLES:
echo   build.cmd GenerateAllTypes
echo   build.cmd AddMigration --migration-name AddPatientNotes --db-context ConsilientDbContext
echo   build.cmd RebuildDatabase --force --backup
echo.
echo DOCUMENTATION:
echo   See build/BUILD.md for full documentation
echo.
exit /b 0
