# Sasogine Localization Source Generator

[Deutsch](LocalizationGenerator.md) · English

The generator creates strongly typed C# keys from `.xdlt` templates and validates `.xdl` translations at build time. The generated code uses Sasogine's localization types.

## Configure the project

Add these groups to the project that will use the keys. The example assumes the two repositories are checked out next to each other. Adjust `SasogineRoot` and add the runtime reference if the project does not already reference Sasogine.

```xml
<PropertyGroup>
  <SasogineRoot>../Sasogine</SasogineRoot>
  <RootNamespace>Sample.Game</RootNamespace>
</PropertyGroup>

<ItemGroup>
  <ProjectReference Include="$(SasogineRoot)/Projects/Basic/Sachssoft.Sasogine.csproj" />
  <ProjectReference Include="$(SasogineRoot)/Build/Sachssoft.Sasogine.Analyzers/Sachssoft.Sasogine.Analyzers.csproj"
                    OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
  <CompilerVisibleProperty Include="RootNamespace" />
  <AdditionalFiles Include="Localization/**/*.xdlt;Localization/**/*.xdl" />
</ItemGroup>
```

`AdditionalFiles` passes the files to the generator. If the application loads translations from embedded resources at runtime, include those `.xdl` files as `EmbeddedResource` items as well.

## Create a template and translation

`Localization/editor.xdlt`

```xml
<dictionary name="Editor" language="en" namespace="Sample.Game.Localization">
  <section name="Menus">
    <item name="menu.title">Main Menu</item>
    <asset name="menu.logo" source="file">Images/logo_en.png</asset>
  </section>
</dictionary>
```

`Localization/editor.de.xdl`

```xml
<dictionary name="Editor" language="de">
  <item name="menu.title">Hauptmenü</item>
</dictionary>
```

- Both files require `name` and `language`. Translations use the same dictionary name and keys as the template. They may omit entries and can be flat or grouped in sections.
- `namespace` is optional in `.xdlt` files. The generator falls back to the project's `RootNamespace`, then to its assembly name.
- Sections can be nested and determine the generated C# class structure. The runtime key remains `menu.title`.
- The generator currently emits `public` classes and fields.

## Use the generated keys

After `dotnet build`, `EditorKeys` is available in the generated namespace. The existing `LocalizationManager` resolves registered translations at runtime.

```csharp
using Sample.Game.Localization;

string title = localization.GetValue(EditorKeys.Menus.MenuTitle);
var logoSource = EditorKeys.Menus.MenuLogo.Fallback;
```

When a text key is missing from the active language, the manager checks the configured fallback language and then uses the text from the `.xdlt` template. An asset entry contains the concrete resource source for its fallback. The application handles loading translations at runtime.

## Select resource sources

`source="file"` selects Sasogine's `LocalFileSource`. A custom source derives from `ResourceSourceBase`, has a public constructor that accepts a single `string`, and can declare an alias with `[ResourceSource("cloud")]`.

```xml
<asset name="menu.banner" source="cloud" provider="Acme.Sasogine.Cloud">banner.png</asset>
```

For third-party sources, set `provider` to the provider's assembly name without the `.dll` extension. The project must reference that assembly. Without `provider`, the generator looks for the alias in Sasogine.

## Inspect the generated code

The generator names each output after its input file. For example, `editor.xdlt` produces `editor.xdlt.cs`. With `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>`, the .NET SDK writes it to the generator's output directory under `obj`.

A separate build step can copy the output next to `editor.xdlt`. If that project also runs the generator, exclude the copy from its `Compile` items so the class is compiled once.

## Common diagnostics

| Code | Meaning |
|---|---|
| `SASLOC001` | The XML is invalid or the file could not be read. |
| `SASLOC002` | The dictionary structure or metadata is invalid. |
| `SASLOC003` | A key appears more than once in the same dictionary. |
| `SASLOC004` | Two elements generate the same C# name in the same scope. |
| `SASLOC005` | Multiple `.xdlt` files declare the same dictionary name. |
| `SASLOC006` | A resource source or provider is missing, unknown, or ambiguous. |
| `SASLOC007` | The template is missing, the language is duplicated, the key is unknown, or the entry kind differs. |
| `SASLOC008` | The generated type name conflicts with another type. |
