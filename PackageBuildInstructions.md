# Package Build and Publish Guide

## Configuration

Set the package version and NuGet API key:

```powershell
$version = "<version>"
$apiKey = "<api-key>"
```

Example:

```powershell
$version = "0.0.4-alpha"
$apiKey = "YOUR_NUGET_API_KEY"
```

## Build Packages

Navigate to the project directory:

```powershell
cd <project-directory-path>
```

Pack the projects:

```powershell
dotnet pack .\Targets\DesktopGL\Sachssoft.Sasogine.DesktopGL\Sachssoft.Sasogine.DesktopGL.csproj -c Release -o .\Packages
```

```powershell
dotnet pack .\Targets\DesktopGL\Sachssoft.Sasogine.Extensions.Sasodoc.DesktopGL\Sachssoft.Sasogine.Extensions.Sasodoc.DesktopGL.csproj -c Release -o .\Packages
```

```powershell
dotnet pack .\Targets\DesktopGL\Sachssoft.Sasogine.Markup.DesktopGL\Sachssoft.Sasogine.Markup.DesktopGL.csproj -c Release -o .\Packages
```

```powershell
dotnet pack .\Targets\DesktopGL\Sachssoft.Sasogine.Toolkit.DesktopGL\Sachssoft.Sasogine.Toolkit.DesktopGL.csproj -c Release -o .\Packages
```

```powershell
dotnet pack .\Targets\DesktopGL\Sachssoft.Sasogine.UI.DesktopGL\Sachssoft.Sasogine.UI.DesktopGL.csproj -c Release -o .\Packages
```

## Publish Packages

Push the generated packages to NuGet.org:

```powershell
dotnet nuget push ".\Packages\Sachssoft.Sasogine.DesktopGL.$version.nupkg" `
    --api-key $apiKey `
    --source "https://api.nuget.org/v3/index.json"
```

```powershell
dotnet nuget push ".\Packages\Sachssoft.Sasogine.Markup.DesktopGL.$version.nupkg" `
    --api-key $apiKey `
    --source "https://api.nuget.org/v3/index.json"
```

```powershell
dotnet nuget push ".\Packages\Sachssoft.Sasogine.Extensions.Sasodoc.DesktopGL.$version.nupkg" `
    --api-key $apiKey `
    --source "https://api.nuget.org/v3/index.json"
```

```powershell
dotnet nuget push ".\Packages\Sachssoft.Sasogine.Toolkit.DesktopGL.$version.nupkg" `
    --api-key $apiKey `
    --source "https://api.nuget.org/v3/index.json"
```

```powershell
dotnet nuget push ".\Packages\Sachssoft.Sasogine.UI.DesktopGL.$version.nupkg" `
    --api-key $apiKey `
    --source "https://api.nuget.org/v3/index.json"
```

## Troubleshooting Version Issues

If a newly published package version is not found, clear the local NuGet caches:

```powershell
dotnet nuget locals all --clear
```

Restore packages without using the cache:

```powershell
dotnet restore --no-cache
```
