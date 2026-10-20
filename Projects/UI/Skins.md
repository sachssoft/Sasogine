# Controls skins

Five interchangeable skins are included:

| Folder | Appearance |
| --- | --- |
| `SachssoftDark` | Original dark skin with cyan accents |
| `SachssoftLight` | Light surfaces with blue accents |
| `SachssoftModern` | Rounded charcoal surfaces with violet accents |
| `SachssoftFuturistic` | Angular navy frames with cyan accents |
| `SachssoftMedieval` | Parchment, wood grain and bronze frames |

Each folder contains a PNG atlas, an atlas XML and a skin XML. The four new skins cover the registered control styles, including extended controls. Each atlas has 139 frames at 1024 × 512 pixels, with premultiplied alpha, edge extrusion, no trimming and no rotation. Color-picker ramps retain their mathematical colors.

## Loading

```csharp
using Sachssoft.Engine.Controls.Styling;

// Keep the skin alive while the root uses its texture.
_skin = ControlSkin.Load(GraphicsDevice,
    Path.Combine(AppContext.BaseDirectory, "Skins", "SachssoftLight"));
_workspace.SetSkin(_skin);
```

Copy the complete skin folder beside the executable. `Load` reads the single `*.skin.xml` manifest, then its atlas and PNG filenames. An overload accepts an explicit skin manifest filename when a folder contains multiple manifests. XML loading and style application use explicit code without reflection and work with NativeAOT.

To switch skins, load the next skin, call `SetSkin`, then dispose the previous skin. A switch restores properties contributed by the previous skin before applying the new one. Local control property overrides remain active. Apply the new palette separately to application-specific styles; the gallery demonstrates this with `ApplyPalette`.

## Example

`Beginner.09.Controls` accepts `--skin dark|light|modern|futuristic|medieval`. The **Skin** menu on the Controls page switches themes at runtime. All gallery pages use the selected palette.

## Rebuilding assets

Run `python Skins/BuildSkins.py` with Pillow installed. `SkinPrimitives.py` supplies the original procedural shapes and glyphs. The script rebuilds the four additional skins from the current Dark manifest and the default styles in `UIRegistry.cs`. Python and Pillow are build tools only; applications do not need them.
