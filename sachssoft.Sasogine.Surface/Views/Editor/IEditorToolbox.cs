using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Interactions;
using System;

namespace Sachssoft.Sasogine.Surface.Views.Editor;

public interface IEditorToolbox
{
    EditorViewIconSizes IconSize { get; set; }
    short Spacing { get; set; }
    bool IsVisible { get; set; }
    short Columns { get; set; }
    void AddButton(string label, Texture2D icon, ICommand command);
    void AddToggleButton(string label, Texture2D icon, ICommand command, bool is_checked = false);
    void AddGroupButton(string label, Texture2D icon, string group, ICommand command, bool is_checked = false);
    void AddSeparator();
    void AddBreak();
    void Clear();
}
