namespace Sachssoft.Sasogine.Graphics.Rendering;

public enum StretchMode
{
    None,       // Keine Skalierung, Originalgröße
    Fill,       // Füllt die Fläche komplett, verzerrt ggf.
    Uniform,    // Skaliert proportional, passt in die Fläche
    UniformToFill // Skaliert proportional, füllt die Fläche komplett, evt. Beschnitt
}