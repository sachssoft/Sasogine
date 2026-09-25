$ErrorActionPreference = "Stop"

$Project = Join-Path $PSScriptRoot "SasogineShaderGenerator.csproj"
$Output = Join-Path $PSScriptRoot "publish\win-x64"

Write-Host "Sachssoft Shader Generator"
Write-Host "Publishing Native AOT for Windows x64..."
Write-Host ""

if (Test-Path $Output)
{
    Write-Host "Cleaning previous publish..."
    Remove-Item $Output -Recurse -Force
}

dotnet publish $Project `
    -c Release `
    -r win-x64 `
    -o $Output

if ($LASTEXITCODE -ne 0)
{
    Write-Host ""
    Write-Host "Publish failed."
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "Publish successful:"
Write-Host $Output

Invoke-Item $Output