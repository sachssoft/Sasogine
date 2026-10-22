@echo off
cd /d "%~dp0publish\win-x64"
09.ControlsGL.exe --page fonts
if errorlevel 1 pause
