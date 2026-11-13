# Sachssoft.Sasogine.Templates

Six project templates for Sasogine. The shown template names do not use the `(Sachssoft)` suffix.

| Template | Command | Backend choice |
|---|---|---|
| Sasogine Empty | `sasogine-empty` | DesktopGL, WindowsDX, WindowsDX12, DesktopVK |
| Sasogine Empty with Scene | `sasogine-scene` | DesktopGL, WindowsDX, WindowsDX12, DesktopVK |
| Sasogine Desktop | `sasogine-desktop` | DesktopGL, WindowsDX, WindowsDX12, DesktopVK |
| Sasogine Editor Desktop | `sasogine-editor` | DesktopGL, WindowsDX, WindowsDX12, DesktopVK |
| Sasogine Crossplatform | `sasogine-crossplatform` | DesktopGL, DesktopVK (experimental) |
| Sasogine Mobile | `sasogine-mobile` | OpenGL ES only (current Android/iOS support) |

- **Project name:** `-n` determines the project filename and assembly name.
- **Root Namespace:** optional textbox (`--RootNamespace`). When omitted, automatically uses the project name, converted to a valid C# namespace.
- **Sasogine Version:** `current` is imported from `Sachssoft.Sasogine.Package.props` when packing; `0.14.1-alpha` is the oldest supported choice. No 0.14.0 option.
- **Desktop graphics:** `--Backend` chooses a Sasogine NuGet backend, not just a runtime flag. `MonoGamePlatform` is set to `Windows` for WindowsDX and to the matching name for others.
- **Mobile:** `--MobilePlatform` selects Android, iOS, or both; `--MobileGraphics OpenGLES` is the currently available option. `DesktopVK` is not a substitute for an Android Vulkan package.

## Build and test locally (without uploading to NuGet.org)

From `D:\Projekte\Sasogine`:

```powershell
dotnet pack .\Projects\Templates\Sachssoft.Sasogine.Templates.csproj -c Release
dotnet new uninstall Sachssoft.Sasogine.Templates
dotnet new install .\Packages\Sachssoft.Sasogine.Templates.0.14.1-alpha.nupkg
dotnet new list sasogine
dotnet new sasogine-scene -n TestGame -o .\TemplateTests\TestGame --RootNamespace Demo.TestGame --Framework net10.0 --Backend DesktopGL --SasogineVersion current
dotnet build .\TemplateTests\TestGame\TestGame.csproj -c Release
```

The `uninstall` step above is optional if the package was not installed yet. When updating from an older local package install, uninstall the old package first (or install the new file directly and follow CLI instructions).

## Repository dependency

The packaging `.csproj` imports `..\..\Sachssoft.Sasogine.Package.props`, reads `VersionPrefix`, `VersionSuffix`, and `MonoGameVersion` and updates only staged copies of each `Sasogine.TemplateVersions.props`. Generated projects do **not** import files from the Sasogine source repository.

## Validation

The archive is statically checked for XML and JSON syntax, references, symbol replacements, and forbidden legacy version choices. Full `dotnet new` and builds must be run on a machine with .NET SDK and mobile workloads installed. The mobile template remains experimental until platform lifecycle and package compatibility are verified.
