@echo off
cd /d "%~dp0publish\win-x64"
"%~dp0publish\win-x64\Sachssoft.LogoInspector.Example.exe"  %*
if errorlevel 1 pause
