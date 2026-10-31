# Frame sets with multiple textures

`KeyedFrameSet`, `IndexedFrameSet`, `IndexedFrameSet<TEnum>`, `KeyedTileFrameSet`,
`IndexedTileFrameSet<TEnum>`, and `SequentialTileFrameSet` accept either one
`Texture2D` or an ordered `IReadOnlyList<Texture2D>`.

Each `FrameData` and `TileFrameData` has a zero-based `TextureIndex`.
Existing constructors and `Add` overloads default to page **0**.
Texture references are copied into a read-only collection; the frame set never
disposes the textures. Frame keys remain unique across all pages.

## Create and resolve frames

```csharp
var frames = new KeyedFrameSet(new[] { texture0, texture1 });
frames.Add("normal", new PixelPoint2(0, 0), new PixelSize2(32, 32));
frames.Add("selected", new PixelPoint2(0, 0), new PixelSize2(32, 32), textureIndex: 1);

FrameData frame = frames["selected"];
Texture2D texture = frames.GetTexture(frame);
PixelBounds2 source = frame.SourceBounds;
```

Use the resolved texture for drawing or cropping. `frames.Texture` remains a
compatibility shortcut for page **0**; it does not resolve a particular frame.
`frames.Textures[frame.TextureIndex]` is an equivalent direct lookup.
The inspector preserves the page index when changing frame coordinates or size.

For tile frame sets, pass `textureIndex` after the cell coordinate. All pages use
the frame set's common tile size. Converting integer frames with
`ToEnumIndexed<TEnum>()` preserves both the textures and each frame's page index.

`MultiFrameBatch` resolves the texture and groups frames by page automatically:

```csharp
batch.Begin(shader, camera);
batch.AddFrame(frames, frames["selected"], position, Color.White);
batch.End();
```

The existing batch ordering contract still applies: order is preserved within a
texture group, but different texture groups can render out of insertion order.

## Import

XML supports one `TextureAtlas` or a container with ordered `TextureAtlas`
children. JSON supports the existing `frames` array, a `frames` object keyed by
frame name, a root array of pages, or a root `textures` array whose page objects
each contain `frames`. Explicit nonnegative `textureIndex` or `page` values on
an atlas/page or a frame override its enclosing page index.

Pass textures in document page order:

```csharp
var importer = FrameSetImporter.Create(DocumentFormatType.Xml, atlasSource);
KeyedFrameSet frames = importer.ToKeyed(new[] { texture0, texture1 });
```

For separate single-page files, import them in the same order as their textures:

```csharp
FrameSetImporter[] pages =
[
    FrameSetImporter.Create(DocumentFormatType.Xml, source0),
    FrameSetImporter.Create(DocumentFormatType.Xml, source1)
];
KeyedFrameSet frames = pages.ImportPages().ToKeyed(new[] { texture0, texture1 });
```

An empty page keeps its position in that order. Duplicate frame keys, missing
textures, and invalid page indices fail explicitly instead of silently using
the first texture.

## Assets

All three frame-set asset classes support ordered texture references through
their new constructors and through `Definition.Textures`. A nonempty `Textures`
list takes precedence over the existing `Definition.Texture` reference.

```csharp
var asset = new KeyedFrameSetAsset("buttons", source0,
    new[]
    {
        new Reference<Texture2DAsset>("buttons.texture0"),
        new Reference<Texture2DAsset>("buttons.texture1")
    });

asset.Definition.FormatType = DocumentFormatType.Xml;

// Only needed when source0 is the first of separate single-page documents.
asset.AdditionalSources.Add(source1);
```

Configure sources before loading the asset through its normal asset context.
With `AdditionalSources`, provide exactly one texture per document: the primary
`LoaderSource` is page 0, followed by the additional sources. With a single
multi-page document, leave `AdditionalSources` empty. Every texture page is
tracked as a reload dependency; unloading a frame-set asset does not dispose
the texture assets it borrows.

Custom `IFrameSet` and `ITileFrameSet` implementations must implement the new
`Textures` property and `GetTexture(frame)` method. Existing implementations in
Sasogine have been updated. No reflection or runtime type discovery is added.
