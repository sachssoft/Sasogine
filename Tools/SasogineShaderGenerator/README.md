# Sasogine Shader Generator

Small Avalonia UI for compiling MonoGame `.fx` effects with `mgfxc`.

## Requirements

- .NET 8 SDK
- MonoGame `mgfxc` available either on `PATH` or entered as an absolute path in the UI.

## Targets

The first version intentionally exposes the actual MonoGame MGFX profiles:

- OpenGL
- DirectX 11

Vulkan is **not** listed because standard MonoGame MGFX does not provide a Vulkan profile. Adding Vulkan would require a separate SPIR-V/Vulkan backend rather than pretending `/Profile:Vulkan` exists.

## Run

```text
dotnet restore
dotnet run
```

The compiler invocation is equivalent to:

```text
mgfxc input.fx output.mgfx /Profile:OpenGL
```

or:

```text
mgfxc input.fx output.mgfx /Profile:DirectX_11
```
