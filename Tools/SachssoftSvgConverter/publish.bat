@echo off
setlocal
cd /d "%~dp0"

echo.
echo Publishing project...
echo.

powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0publish.ps1"
set "EXITCODE=%ERRORLEVEL%"

if not "%EXITCODE%"=="0" (
    echo.
    echo Publish FAILED with exit code %EXITCODE%.
    pause
    exit /b %EXITCODE%
)

echo.
echo Publish completed successfully.
pause
exit /b 0
