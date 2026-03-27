using Sachssoft.Sasogine.Editor.Tiling;
using Sachssoft.Sasogine.Tiling;
using System;
using Sachssoft.Sasogine.Editor.Tiles.Tools;

public class EditorToolSpray : EditorToolBrush
{
    private int _saturation_value = 10;
    private readonly Random _rnd = new Random();

    public EditorToolSpray()
    {
    }

    protected override void PaintAt(Coordinate coordinate, TileEditorRuntimeBase runtime)
    {
        if (CanSetTileBySaturation())
            base.PaintAt(coordinate, runtime);
    }

    private bool CanSetTileBySaturation()
    {
        // Wahrscheinlichkeit in Prozent
        return _rnd.Next(0, 100) < _saturation_value;
    }

    public int Saturation
    {
        get => _saturation_value;
        set => _saturation_value = int.Clamp(value, 0, 100);
    }
}
