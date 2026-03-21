using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Interactions;
using System.Windows.Input;

namespace Sachssoft.Sasogine.Surface.Scenes.Editor;

public interface IEditorMenu
{
    void Add(string label, ICommand command);
    void AddToggle(string label, ICommand command);
    void AddGroup(string label, string group, ICommand command);
    void AddSeparator();
    IEditorMenu AddChild(string label);
    void Clear();

}
