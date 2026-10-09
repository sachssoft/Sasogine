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

The output file is generated beside the selected `.fx` source. The application invokes:

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
