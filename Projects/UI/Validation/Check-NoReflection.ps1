$ErrorActionPreference = 'Stop'
$uiRoot = Split-Path -Parent $PSScriptRoot
$patterns = @('\bSystem\.Reflection\b','\btypeof\s*\(','\.\s*GetType\s*\(','\bActivator\s*\.','\bSystem\.Linq\.Expressions\b','\b(DynamicMethod|DispatchProxy)\b')
$files = @(Get-ChildItem -LiteralPath $uiRoot -Filter '*.cs' -Recurse -File | Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' })
$found = @($files | Select-String -Pattern $patterns)
if ($found.Count -gt 0) { $found; throw 'Forbidden reflection or runtime discovery found.' }
Write-Output "PASS: $($files.Count) source files checked for forbidden reflection APIs."
