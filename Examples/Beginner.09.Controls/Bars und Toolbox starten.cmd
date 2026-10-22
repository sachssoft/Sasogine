@echo off
setlocal
pushd "%~dp0publish\win-x64"
if errorlevel 1 exit /b 1
"%~dp0publish\win-x64\09.ControlsGL.exe" --page bars %*
set "exampleResult=%errorlevel%"
popd
if not "%exampleResult%"=="0" pause
exit /b %exampleResult%
