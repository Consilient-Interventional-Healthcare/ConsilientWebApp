@echo off
REM ============================================================================
REM REBUILD DATABASE UTILITY
REM ============================================================================
REM
REM PURPOSE: Shortcut to run the RebuildDatabase build target.
REM          Drops all objects and rebuilds database from SQL scripts.
REM          Faster than ResetDatabase as it doesn't recreate the Docker container.
REM
REM USAGE (from repository root):
REM     scripts\rebuild-database.cmd
REM     scripts\rebuild-database.cmd --backup
REM
REM DO NOT USE:
REM     ./scripts/rebuild-database.cmd           (forward slashes fail on Windows)
REM     cd scripts && rebuild-database.cmd       (wrong working directory)
REM     bash scripts/rebuild-database.cmd        (wrong shell interpreter)
REM
REM OPTIONS:
REM     --backup    Create a backup before dropping objects (saved to temp dir)
REM
REM WHAT HAPPENS:
REM     1. Ensures database container is running (if Docker)
REM     2. Creates backup (if --backup flag provided)
REM     3. Drops all objects: FKs, Views, Procedures, Functions, Tables, Types, Schemas
REM     4. Runs all SQL scripts in src\Databases\consilient_main\ in order
REM     5. Verifies database health (table count)
REM
REM ============================================================================

setlocal

REM Check for --backup flag
set "BACKUP_FLAG="
if /i "%~1"=="--backup" set "BACKUP_FLAG=--backup"

echo.
echo ============================================
echo Rebuild Database Utility
echo ============================================
echo.
echo This will:
echo   1. Drop ALL objects from consilient_main database
echo   2. Run all SQL scripts to recreate the schema
if defined BACKUP_FLAG (
    echo   3. Create a backup before dropping objects
)
echo.
echo Press Ctrl+C to cancel, or any key to continue...
pause >nul

echo.
if defined BACKUP_FLAG (
    echo Running: .\build.ps1 RebuildDatabase --force --backup
    echo.
    powershell.exe -ExecutionPolicy Bypass -Command "& { .\build.ps1 RebuildDatabase --force --backup }"
) else (
    echo Running: .\build.ps1 RebuildDatabase --force
    echo.
    powershell.exe -ExecutionPolicy Bypass -Command "& { .\build.ps1 RebuildDatabase --force }"
)

if errorlevel 1 (
    echo.
    echo ERROR: RebuildDatabase failed. See output above for details.
    exit /b 1
)

echo.
echo ============================================
echo Rebuild completed successfully!
echo ============================================
echo.

exit /b 0
