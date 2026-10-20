@echo off
cd /d "%~dp0publish\win-x64"
"%~dp0publish\win-x64\Sachssoft.LogoInspector.Example.exe" --values Insets3 %*
if errorlevel 1 pause
