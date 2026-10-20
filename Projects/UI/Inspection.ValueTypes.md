# Registered inspector value types

The built-in `InspectorEditorRegistry` contains 76 explicit closed-type registrations and one automatic enum fallback. It covers the numeric composite editors from the earlier Myra inspector and additional common engine values. Normal factories use generic keys and typed delegates. The enum fallback checks `typeof(T).IsEnum` and caches declared names and values once per closed type using `Enum.GetNames` and `Enum.Parse`; no source property discovery or runtime code generation is performed.

```csharp
var inspector = new InspectorView();
inspector.Add("Position", () => definition.Position,
    value => definition.Position = value, definition, nameof(definition.Position));
```

The compile-time value type selects the registered editor. The property is still explicitly bound; its source members are not discovered automatically.

| Family | Built-in types |
| --- | --- |
| Points | MonoGame `Point`; `Point2`, `Point3`, `PixelPoint2`, `PixelPoint3`, `Coordinate2`, `Coordinate3` |
| Sizes | `Size2`, `Size3`, `PixelSize2`, `PixelSize3` |
| Bounds | `Bounds2`, `Bounds3`, `PixelBounds2`, `PixelBounds3`, MonoGame `Rectangle` |
| Boxes | `Box2`, `Box3`, `PixelBox2`, `PixelBox3` |
| Insets | `Insets2`, `Insets3`, `PixelInsets2`, `PixelInsets3` |
| Vectors and transforms | MonoGame `Vector2`, `Vector3`, `Vector4`, `Quaternion`, `Matrix` |
| Geometric values | `Segment`, MonoGame `Ray`, `Plane`, `BoundingBox`, `BoundingSphere` |
| Colors | MonoGame `Color`; `NormalizedColor`, `RgbColor`, `HslColor`, `HsvColor`, `LinearRgbColor`, `OklabColor` |
| Normalized values | `Normalized`, `SignedNormalized` |
| Atlas regions | `FrameData`, `TileFrameData` |
| Gameplay values | `LifeInfo`, `CollectibleInfo`, `CountdownInfo`; low/high tiered scores for the numeric types below |
| Scalar numbers | `sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `nint`, `nuint`, `Int128`, `UInt128`, `Half`, `float`, `double`, `decimal`, `BigInteger` |
| Other values | `string`, `bool`, `TimeSpan`, `DateTime`, `DateTimeOffset`, `Guid`, `NamedGameRegistryKey` |
| Enums | Any statically bound enum, including application enums; no individual registration |

`LowTieredScore<T>` and `HighTieredScore<T>` have built-in registrations only for `int` and `float`. A registry can register scores for another compatible number type with `RegisterScores<TNumber>("Name")`.

## Grouping

Points and vectors share one component row. Bounds and rectangles use **Position / Size**, boxes use **Min / Max**, and 3D insets use **Start / End**. Insets retain the engine's Left, Top, Front, Right, Bottom, Back constructor order; `Bt` distinguishes Bottom from Back (`Bk`). Matrix components are grouped by row. The full struct remains one binding, so changing one coordinate preserves other coordinates and rows, including pending external changes.

## Numeric accuracy

Registered numeric composites use `TypedMultiValueEditor<T,TNumber>` and `TypedValueTextBox<TNumber>`. Values are parsed and formatted in their actual numeric type with invariant culture. Int32/Int64/UInt64 coordinates do not pass through `float`; doubles do not pass through `float` or `decimal`. Enter or focus loss commits text. Escape cancels; overflow and invalid text preserve the source. Engine constructors retain their normalization rules, including color and tier-score normalization.

For deliberately bounded drag/slider fields, the earlier `AddSingle`, `AddVector2/3/4` and `AddValues` APIs remain available. They use the existing float `ValueTextBox`. Central styles remain applicable to both field types.

## Custom registration

Each workspace owns an independent `Registry.InspectorEditors`, initialized with the defaults. Detached inspectors use immutable defaults unless supplied with a registry explicitly. Factories are selected when `Add` is called. Supply a custom registry when building custom editors before workspace attachment; nested groups share it.

```csharp
var editors = new InspectorEditorRegistry();
editors.Register<MyValue>("MyValue", (view, title, read, write, source, name) =>
    view.AddParsed(title, read, write, FormatMyValue, TryParseMyValue, source, name));
var inspector = new InspectorView(editors);
```

Enums work directly through `inspector.Add("Mode", () => definition.Mode, value => definition.Mode = value)`. `RegisterChoices` optionally overrides the automatic choices with a filtered or localized list. Declared aliases and flags constants are listed; the combo box does not compose arbitrary flag combinations. Undefined values remain unchanged with no selected item. `Count` and `Names` describe explicit registrations only. Enum metadata is cached per type, outside frame updates. Nullable enum values are not automatically supported. Other application structs, runtime snapshots, references and specialized objects require an explicit appropriate factory; there is no fallback that inspects their members.

`Advanced.08.LogoInspector` has a **Value types** tab with a retained example for every default registration. It builds only the selected editor. Start with `--values Bounds3` or `--values PixelPoint3` to open one directly.
