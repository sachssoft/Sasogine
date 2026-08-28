# Package Build and Publish Guide

## Build Packages

PowerShell:

1. Navigate to the project directory:

```powershell
cd <project-directory-path>
```

2. Pack the projects:

```powershell
dotnet pack .\Targets\DesktopGL\Sachssoft.Sasogine.DesktopGL\Sachssoft.Sasogine.DesktopGL.csproj -c Release -o .\Packages
```

```powershell
dotnet pack .\Targets\DesktopGL\Sachssoft.Sasogine.Extensions.Sasodoc.DesktopGL\Sachssoft.Sasogine.Extensions.Sasodoc.DesktopGL.csproj -c Release -o .\Packages
```

```powershell
dotnet pack .\Targets\DesktopGL\Sachssoft.Sasogine.Toolkit.DesktopGL\Sachssoft.Sasogine.Toolkit.DesktopGL.csproj -c Release -o .\Packages
```

```powershell
dotnet pack .\Targets\DesktopGL\Sachssoft.Sasogine.UI.DesktopGL\Sachssoft.Sasogine.UI.DesktopGL.csproj -c Release -o .\Packages
```

## Publish Packages

3. Push the generated packages to NuGet.org:

```powershell
dotnet nuget push ".\Packages\Sachssoft.Sasogine.DesktopGL.<version>.nupkg" `
    --api-key YOUR_NUGET_API_KEY `
    --source "https://api.nuget.org/v3/index.json"
```

```powershell
dotnet nuget push ".\Packages\Sachssoft.Sasogine.Extensions.Sasodoc.DesktopGL.<version>.nupkg" `
    --api-key YOUR_NUGET_API_KEY `
    --source "https://api.nuget.org/v3/index.json"
```

## Troubleshooting Version Issues

If a newly published package version is not found:

1. Clear the local NuGet caches:

```powershell
dotnet nuget locals all --clear
```

2. Restore packages without using the cache:

```powershell
dotnet restore --no-cache
```
