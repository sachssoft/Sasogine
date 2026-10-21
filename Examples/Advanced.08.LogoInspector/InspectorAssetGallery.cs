using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Assets.Graphics;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Demonstrates cached texture tiles, list selection and a typed reference property.</summary>
internal sealed class InspectorAssetGallery : ContentControl, IDisposable
{
    private GraphicsDevice? _graphics;
    private readonly List<Texture2D> _textures = new();
    private readonly AssetItem[] _assets;
    internal InspectorView Inspector { get; } = new();
    internal AssetView Assets { get; } = new()
    {
        MultiSelect = true
    };
    internal ReferencePicker<Texture2DAsset> Reference { get; }

    internal InspectorAssetGallery(LogoAppearanceDefinition definition)
    {
        _assets = new AssetItem[25];
        for (int i = 0; i < 24; i++)
        {
            int number = i;
            var asset = new Texture2DAsset("example.texture." + number);
            _assets[i] = new AssetItem(asset, "Material " + (number + 1).ToString("00"), "Texture", () => MakePreview(number));
        }

        _assets[24] = new AssetItem(new AssetExampleObject("example.entity"), "Scene object", "Entity");
        Assets.Source = _assets;
        var grid = new Grid
        {
            Spacing = 8,
            Padding = new Insets2(12)
        };
        grid.Rows.Add(new GridRow { Height = new GridLength(26) });
        grid.Rows.Add(new GridRow { Height = new GridLength(34) });
        grid.Rows.Add(new GridRow { Height = GridLength.Proportion() });
        grid.Rows.Add(new GridRow { Height = new GridLength(48) });
        grid.Rows.Add(new GridRow { Height = new GridLength(78) });
        grid.Rows.Add(new GridRow { Height = new GridLength(34) });
        var gridChild1 = new TextBlock
        {
            Text = "ASSETS · cached previews"
        };
        grid.Children.Add(gridChild1);
        grid.Cells.Put(gridChild1, 0);
        var toolbar = new Grid
        {
            Spacing = 6
        };
        toolbar.Columns.Add(new GridColumn { Width = GridLength.Proportion() });
        toolbar.Columns.Add(new GridColumn { Width = new GridLength(92) });
        var search = new TextBox
        {
            ToolTip = "Search name, identifier or kind"
        };
        search.TextChanged += (_, _) => Assets.FilterText = search.Text;
        var view = new Button
        {
            Content = new TextBlock
            {
                Text = "Tiles / List"
            }
        };
        view.Clicked += (_, _) => Assets.ViewMode = Assets.ViewMode == AssetViewMode.Tiles ? AssetViewMode.List : AssetViewMode.Tiles;
        toolbar.Children.Add(search);
        toolbar.Cells.Put(search);
        toolbar.Children.Add(view);
        toolbar.Cells.Put(view, column: 1);
        grid.Children.Add(toolbar);
        grid.Cells.Put(toolbar, 1);
        grid.Children.Add(Assets);
        grid.Cells.Put(Assets, 2);
        var inspector = Inspector;
        Reference = inspector.AddReference("Texture", () => definition.PreviewTexture, value => definition.PreviewTexture = value, _assets, definition, nameof(definition.PreviewTexture));
        grid.Children.Add(inspector);
        grid.Cells.Put(inspector, 3);
        var gridChild2 = new TextBlock
        {
            Text = "Ctrl / Shift: multi-select · Drag a tile to Texture.\nBrowse filters texture assets. Missing IDs stay visible.\nColor dialog: Pick pixel samples the logo on the left.",
            WrapText = true
        };
        grid.Children.Add(gridChild2);
        grid.Cells.Put(gridChild2, 4);
        var color = new Button
        {
            Content = new TextBlock
            {
                Text = "Color picker & render-target pipette"
            }
        };
        color.Clicked += (_, _) =>
        {
            if (Workspace is not Workspace owner)
                return;
            var dialog = new ColorPickerDialog
            {
                SelectedColor = definition.Primary
            };
            dialog.Closed += (_, _) =>
            {
                if (dialog.Result == DialogResult.OK && Workspace == owner)
                    definition.Primary = dialog.SelectedColor;
            };
            _ = owner.ShowDialog(dialog);
        };
        grid.Children.Add(color);
        grid.Cells.Put(color, 5);
        Content = grid;
    }

    internal void SetGraphicsDevice(GraphicsDevice graphics)
    {
        _graphics = graphics;
        foreach (AssetItem asset in _assets)
            asset.InvalidatePreview();
    }

    private AssetPreview? MakePreview(int number)
    {
        if (_graphics is null)
            return null;
        var texture = new Texture2D(_graphics, 64, 64);
        var pixels = new Color[64 * 64];
        Color tint = Color.Lerp(new Color(65, 199, 223), new Color(192, 92, 223), number / 23f);
        for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                bool pattern = number % 3 == 0 ? (x / 8 + y / 8) % 2 == 0 : number % 3 == 1 ? (x - 32) * (x - 32) + (y - 32) * (y - 32) < 650 : (x + y) % 18 < 9;
                pixels[y * 64 + x] = pattern ? tint : Color.Lerp(tint, new Color(20, 28, 40), .7f);
            }

        texture.SetData(pixels);
        _textures.Add(texture);
        return new AssetPreview(texture);
    }

    public void Dispose()
    {
        foreach (Texture2D texture in _textures)
            texture.Dispose();
        _textures.Clear();
    }
}
