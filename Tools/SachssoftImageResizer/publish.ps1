[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$ProjectFile = Join-Path $PSScriptRoot 'Sachssoft.ImageResizer.csproj'
$OutputDir = Join-Path $PSScriptRoot 'publish\win-x64'

Write-Host ''
Write-Host 'Publishing Windows x64 Native AOT...'
Write-Host "Project: $ProjectFile"
Write-Host "Output : $OutputDir"
Write-Host ''

if (-not (Test-Path -LiteralPath $ProjectFile)) {
    Write-Error "Project file not found: $ProjectFile"
    exit 1
}

try {
    Get-Command dotnet -ErrorAction Stop | Out-Null
}
catch {
    Write-Error 'dotnet was not found in PATH. Install the .NET SDK or fix PATH.'
    exit 1
}

if (Test-Path -LiteralPath $OutputDir) {
    Write-Host 'Cleaning previous publish output...'
    Remove-Item -LiteralPath $OutputDir -Recurse -Force
}

$PublishArgs = @(
    'publish'
    $ProjectFile
    '-c'
    'Release'
    '-r'
    'win-x64'
    '--self-contained'
    'true'
    '-p:PublishAot=true'
    '-o'
    $OutputDir
)

& dotnet @PublishArgs
$ExitCode = $LASTEXITCODE

if ($ExitCode -ne 0) {
    Write-Host ''
    Write-Error "Publish failed with exit code $ExitCode."
    exit $ExitCode
}

Write-Host ''
Write-Host 'Publish completed successfully.'
Write-Host "Output: $OutputDir"

exit 0
