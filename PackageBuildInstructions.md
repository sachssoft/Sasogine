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

First, build the main Sasogine package:

```powershell
dotnet pack .\Targets\DesktopGL\Sachssoft.Sasogine.DesktopGL\Sachssoft.Sasogine.DesktopGL.csproj -c Release -o .\Packages
```

Publish the main package to NuGet.org before building the dependent packages:

```powershell
dotnet nuget push ".\Packages\Sachssoft.Sasogine.DesktopGL.$version.nupkg" `
    --api-key $apiKey `
    --source "https://api.nuget.org/v3/index.json"
```

Then build the remaining packages together:

```powershell
dotnet pack .\Targets\DesktopGL\Sachssoft.Sasogine.Extensions.Sasodoc.DesktopGL\Sachssoft.Sasogine.Extensions.Sasodoc.DesktopGL.csproj -c Release -o .\Packages
dotnet pack .\Targets\DesktopGL\Sachssoft.Sasogine.Markup.DesktopGL\Sachssoft.Sasogine.Markup.DesktopGL.csproj -c Release -o .\Packages
dotnet pack .\Targets\DesktopGL\Sachssoft.Sasogine.Toolkit.DesktopGL\Sachssoft.Sasogine.Toolkit.DesktopGL.csproj -c Release -o .\Packages
```

## Publish Remaining Packages

After all remaining packages have been built successfully, publish them together:

```powershell
dotnet nuget push ".\Packages\Sachssoft.Sasogine.Extensions.Sasodoc.DesktopGL.$version.nupkg" `
    --api-key $apiKey `
    --source "https://api.nuget.org/v3/index.json"

dotnet nuget push ".\Packages\Sachssoft.Sasogine.Markup.DesktopGL.$version.nupkg" `
    --api-key $apiKey `
    --source "https://api.nuget.org/v3/index.json"

dotnet nuget push ".\Packages\Sachssoft.Sasogine.Toolkit.DesktopGL.$version.nupkg" `
    --api-key $apiKey `
    --source "https://api.nuget.org/v3/index.json"
```

## Troubleshooting Version Issues

If a newly published package version cannot be found, clear the local NuGet caches:

```powershell
dotnet nuget locals all --clear
```

Then rebuild the remaining projects in Visual Studio using the context menu.

If the package version is still not found, restore the packages without using the local cache:

```powershell
dotnet restore --no-cache
```

This step is normally optional and should only be necessary if clearing the NuGet caches and rebuilding the projects does not resolve the issue.
