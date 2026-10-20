@echo off
setlocal
pushd "%~dp0"
if errorlevel 1 exit /b 1
dotnet publish "01.EditorGL.csproj" -c Release -r win-x64 --self-contained true -p:UseAppHost=true -o "%~dp0publish\win-x64" %*
set "exampleResult=%errorlevel%"
if "%exampleResult%"=="0" echo EXE erstellt unter "%~dp0publish\win-x64"
popd
exit /b %exampleResult%
