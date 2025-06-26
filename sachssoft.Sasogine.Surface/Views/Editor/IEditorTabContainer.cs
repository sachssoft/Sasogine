using Microsoft.Xna.Framework.Graphics;
using sachssoft.Sasogine.Surface.Visuals.Controls;

namespace sachssoft.Sasogine.Surface.Views.Editor;

public interface IEditorTabContainer
{
    EditorViewIconSizes IconSize { get; set; }
    bool IsTabVisible { get; set; }
    bool IsVisible { get; set; }
    void Add(string title, Texture2D icon, Container container);
    void Clear();
}
