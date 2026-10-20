@echo off
setlocal
pushd "%~dp0"
if errorlevel 1 exit /b 1
set "DOTNET_TieredCompilation=0"
dotnet run --project "GameLoop.csproj" -c Release -- %*
set "benchmarkResult=%errorlevel%"
popd
exit /b %benchmarkResult%
