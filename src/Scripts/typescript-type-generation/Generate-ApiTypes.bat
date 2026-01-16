@echo off
REM Generates TypeScript interfaces from Consilient.Api

echo ========================================
echo   Consilient.Api TypeScript Generation
echo ========================================
echo.

REM Check if pwsh (PowerShell 7+) is available
where pwsh >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo Using PowerShell 7+
    echo.
    pwsh -NoProfile -ExecutionPolicy Bypass -File "%~dp0Generate-ApiTypes.ps1" %*
) else (
    echo [ERROR] PowerShell 7+ not found!
    echo.
    echo This script requires PowerShell 7.0 or higher.
    echo.
    echo Install PowerShell 7:
    echo   https://aka.ms/install-powershell
    echo   or via winget: winget install Microsoft.PowerShell
    echo.
    echo After installing, run this batch file again.
    exit /b 1
)
