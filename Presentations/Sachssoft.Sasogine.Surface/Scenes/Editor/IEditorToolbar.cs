using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Interactions;
using Sachssoft.Sasogine.Surface.Controls;
using Sachssoft.Sasogine.Surface.Visuals.Controls;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System.Windows.Input;

namespace Sachssoft.Sasogine.Surface.Scenes.Editor;

public interface IEditorToolbar
{
    short Spacing { get; set; }
    bool IsVisible { get; set; }
    void AddLabel(string text);
    void AddIcon(ITextureRegion icon);
    void AddWidget(Widget widget);
    void AddButton(string label, ITextureRegion icon, ICommand command);
    void AddToggleButton(string label, ITextureRegion icon, ICommand command, bool is_checked = false);
    void AddGroupButton(string label, ITextureRegion icon, string group, ICommand command, bool is_checked = false);
    void AddSeparator();
    void Clear();
}
