@echo off
REM ============================================================================
REM SQUASH MIGRATIONS UTILITY
REM ============================================================================
REM
REM PURPOSE: Shortcut to run the SquashMigrations build target.
REM          Consolidates all EF Core migrations into a single Initial migration.
REM
REM USAGE (from repository root):
REM     scripts\squash-migrations.cmd ConsilientDbContext
REM     scripts\squash-migrations.cmd UsersDbContext
REM
REM DO NOT USE:
REM     ./scripts/squash-migrations.cmd           (forward slashes fail on Windows)
REM     cd scripts && squash-migrations.cmd       (wrong working directory)
REM     bash scripts/squash-migrations.cmd        (wrong shell interpreter)
REM
REM WHAT GETS DELETED:
REM     - C# migration files: src/Consilient.Data.Migrations/{Context}/*.cs
REM     - SQL scripts matching: {NN}_{context}_*.sql
REM
REM WHAT GETS PRESERVED:
REM     - Manual scripts: {NN}_manual_*.sql
REM     - Other context scripts: {NN}_{otherContext}_*.sql
REM     - Seed data: seed.sql
REM
REM ============================================================================

setlocal

REM Check if db-context parameter was provided
if "%~1"=="" (
    echo.
    echo ERROR: Missing required parameter: db-context
    echo.
    echo Usage: scripts\squash-migrations.cmd ^<DbContext^>
    echo.
    echo Examples:
    echo     scripts\squash-migrations.cmd ConsilientDbContext
    echo     scripts\squash-migrations.cmd UsersDbContext
    echo.
    echo Note: "Both" is not allowed for SquashMigrations.
    echo.
    exit /b 1
)

set "DB_CONTEXT=%~1"

REM Validate db-context value
if /i "%DB_CONTEXT%"=="Both" (
    echo.
    echo ERROR: "Both" is not allowed for SquashMigrations.
    echo        Please specify either ConsilientDbContext or UsersDbContext.
    echo.
    exit /b 1
)

echo.
echo ============================================
echo Squash Migrations Utility
echo ============================================
echo.
echo Context: %DB_CONTEXT%
echo.
echo This will:
echo   1. Delete all C# migration files for %DB_CONTEXT%
echo   2. Delete all SQL scripts for %DB_CONTEXT%
echo   3. Create a fresh Initial migration
echo   4. Generate a new SQL script
echo.
echo Press Ctrl+C to cancel, or any key to continue...
pause >nul

echo.
echo Running: .\build.ps1 SquashMigrations --db-context %DB_CONTEXT% --force
echo.

powershell.exe -ExecutionPolicy Bypass -Command "& { .\build.ps1 SquashMigrations --db-context %DB_CONTEXT% --force }"

if errorlevel 1 (
    echo.
    echo ERROR: SquashMigrations failed. See output above for details.
    exit /b 1
)

echo.
echo ============================================
echo Squash completed successfully!
echo ============================================
echo.

exit /b 0
