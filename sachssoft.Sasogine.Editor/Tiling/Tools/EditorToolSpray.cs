using sachssoft.Sasogine.Editor.Tiling.Tools;
using sachssoft.Sasogine.Editor.Tiling;
using sachssoft.Sasogine.Tiling;
using System;

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
