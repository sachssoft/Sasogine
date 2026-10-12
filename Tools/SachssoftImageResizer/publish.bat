@echo off
cd /d "%~dp0"

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0publish.ps1"

if errorlevel 1 (
    echo.
    echo Publish FAILED.
    pause
    exit /b 1
)

echo.
echo Publish completed successfully.
pause