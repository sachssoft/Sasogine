using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace sachssoft.Sasogine.Surface.Forms;

[Obsolete("Remove!!")]
public class ShortcutEvent
{
    private bool[] _key_pressed;
    private List<Tuple<Command, CommandShortcut>> _shortcuts;

    public ShortcutEvent()
    {
        _key_pressed = new bool[(int)Enum.GetValues(typeof(Keys)).Cast<Keys>().Max() + 1];
        _shortcuts = new List<Tuple<Command, CommandShortcut>>();
    }

    public void AddShortcut(Command command, CommandShortcut shortcut)
    {
    }

    public void Update(GameContext context)
    {
        foreach (var shortcut in _shortcuts)
        {
            //if (context.InputEvents.Keyboard.IsKeyDown(shortcut.Item2) == true && _key_pressed[(int)shortcut.Item2] == false)
            //{
            //    shortcut.Item1.Execute(context.View);
            //    _key_pressed[(int)shortcut.Item2] = true;
            //}
            //else if (context.InputEvents.Keyboard.IsKeyUp(shortcut.Item2) == true && _key_pressed[(int)shortcut.Item2] == true)
            //{
            //    _key_pressed[(int)shortcut.Item2] = false;
            //}
        }
    }
}
