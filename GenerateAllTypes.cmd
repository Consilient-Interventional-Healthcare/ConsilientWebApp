@echo off
setlocal

pushd "%~dp0"

echo Building NUKE build project...
dotnet build build\_build.csproj --verbosity quiet
if errorlevel 1 (
    echo Failed to build the build project
    popd
    exit /b %ERRORLEVEL%
)

echo Running NUKE GenerateAllTypes...
build\bin\Debug\_build.exe GenerateAllTypes %*

popd
echo.
pause
exit /b %ERRORLEVEL%
