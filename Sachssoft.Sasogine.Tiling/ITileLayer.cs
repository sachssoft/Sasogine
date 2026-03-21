namespace Sachssoft.Sasogine.Tiling;

public interface ITileLayer
{   
    int Index { get; }

    bool IsVisible { get; set; }

    string? Name { get; set; }
}
