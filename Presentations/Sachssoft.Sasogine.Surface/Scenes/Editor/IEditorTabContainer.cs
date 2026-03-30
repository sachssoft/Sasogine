using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Surface.Controls;
using Sachssoft.Sasogine.Surface.Visuals.Controls;
using Sachssoft.Sasogine.Surface.Visuals.Regions;

namespace Sachssoft.Sasogine.Surface.Scenes.Editor;

public interface IEditorTabContainer
{
    EditorViewIconSizes IconSize { get; set; }
    bool IsTabVisible { get; set; }
    bool IsVisible { get; set; }
    void Add(string title, ITextureRegion icon, Container container);
    void Clear();
}
