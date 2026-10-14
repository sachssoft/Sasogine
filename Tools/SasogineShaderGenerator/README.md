# Sasogine Shader Generator

Avalonia UI for compiling MonoGame `.fx` effects with `mgfxc`.

## Requirements

- .NET 8 SDK (application target unchanged).
- **MonoGame `dotnet-mgfxc` 3.8.6** installed and available on `PATH`, or the path to its executable entered in the UI.
- The generator itself does not reference MonoGame NuGet packages. Updating the **external compiler** to 3.8.6 is what matters.

To update the globally installed compiler:

```powershell
dotnet tool update --global dotnet-mgfxc --version 3.8.6
```

If it is managed by a local tool manifest, run the equivalent `dotnet tool update dotnet-mgfxc --version 3.8.6` in that manifest's directory.

## Targets

| Target | MGFXC profile | Output suffix |
| --- | --- | --- |
| OpenGL | `OpenGL` | `_gl.mgfxo` |
| DirectX 11 | `DirectX_11` | `_dx.mgfxo` |
| DirectX 12 | `DirectX_12` | `_dx12.mgfxo` |
| Vulkan | `Vulkan` | `_vk.mgfxo` |

By default, the output file is generated beside the selected `.fx` source. Enable
**Save compiled shaders in target subfolders (gl, dx, dx12, vk)** to place the
compiled shader into a backend-specific subfolder instead. The application
creates the subfolder automatically when compiling.

For example, for a source at `Shaders/terrain.fx`:

| Target | Checkbox off | Checkbox on |
| --- | --- | --- |
| OpenGL | `Shaders/terrain_gl.mgfxo` | `Shaders/gl/terrain_gl.mgfxo` |
| DirectX 11 | `Shaders/terrain_dx.mgfxo` | `Shaders/dx/terrain_dx.mgfxo` |
| DirectX 12 | `Shaders/terrain_dx12.mgfxo` | `Shaders/dx12/terrain_dx12.mgfxo` |
| Vulkan | `Shaders/terrain_vk.mgfxo` | `Shaders/vk/terrain_vk.mgfxo` |

The output path preview updates immediately when the checkbox or target is changed. The application invokes:

```text
mgfxc input.fx output.mgfxo /Profile:DirectX_12
mgfxc input.fx output.mgfxo /Profile:Vulkan
```

DX12/Vulkan compilation requires a compatible MGFXC version and shader code. A successful compile does not guarantee correct rendering on all GPU drivers or backend implementations.

## Run

```powershell
dotnet restore
dotnet run
```

## Troubleshooting: cannot start `mgfxo`

`mgfxo` is the **output extension**, not the compiler executable. Use `mgfxc` in
the **mgfxc** field. The generator automatically corrects the common `mgfxo`
input to `mgfxc`. On Windows, it also checks the default global tool path
`%USERPROFILE%\.dotnet\tools\mgfxc.exe` if the GUI has an outdated `PATH`.

Install the compiler (if missing):

```powershell
dotnet tool install --global dotnet-mgfxc --version 3.8.6
```

If already installed, use `dotnet tool update --global dotnet-mgfxc --version 3.8.6`.
Restart the app or enter the full path to `mgfxc.exe`.
