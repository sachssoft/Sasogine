using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Interactions;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System;
using System.Windows.Input;

namespace Sachssoft.Sasogine.Surface.Scenes.Editor;

public interface IEditorToolbox
{
    EditorViewIconSizes IconSize { get; set; }
    short Spacing { get; set; }
    bool IsVisible { get; set; }
    short Columns { get; set; }    
    void AddButton(string label, ITextureRegion icon, ICommand command);
    void AddToggleButton(string label, ITextureRegion icon, ICommand command, bool is_checked = false);
    void AddGroupButton(string label, ITextureRegion icon, string group, ICommand command, bool is_checked = false);
    void AddSeparator();
    void AddBreak();
    void Clear();
}
