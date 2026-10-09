$ErrorActionPreference = 'Stop'

$project = Join-Path $PSScriptRoot 'SasogineTextureConverter.csproj'
$destination = Join-Path $PSScriptRoot 'publish\win-x64'

dotnet publish $project -c Release -r win-x64 --self-contained true -p:PublishAot=true -o $destination
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Published to $destination"
