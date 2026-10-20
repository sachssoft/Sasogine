@echo off
setlocal
pushd "%~dp0"
if errorlevel 1 exit /b 1
dotnet publish "Controls.Myra.csproj" -c Release -r win-x64 --self-contained true -p:UseAppHost=true -o "%~dp0publish\rendering-win-x64" %*
set "benchmarkResult=%errorlevel%"
popd
exit /b %benchmarkResult%
