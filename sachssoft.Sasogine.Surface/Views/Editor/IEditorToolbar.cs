using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Interactions;
using Sachssoft.Sasogine.Surface.Visuals.Controls;

namespace Sachssoft.Sasogine.Surface.Views.Editor;

public interface IEditorToolbar
{
    short Spacing { get; set; }
    bool IsVisible { get; set; }
    void AddLabel(string text);
    void AddIcon(Texture2D icon);
    void AddWidget(Widget widget);
    void AddButton(string label, Texture2D icon, ICommand command);
    void AddToggleButton(string label, Texture2D icon, ICommand command, bool is_checked = false);
    void AddGroupButton(string label, Texture2D icon, string group, ICommand command, bool is_checked = false);
    void AddSeparator();
    void Clear();
}
