/* 
 * © 2024 Tobias Sachs
 * CommandShortcut
 * 11.07.2024 
*/

using Microsoft.Xna.Framework.Input;
using System;

namespace Sachssoft.Sasogine.Surface.Forms;

[Obsolete("Remove!!")]
public class CommandShortcut
{
    [Flags]
    public enum KeyModifiers
    {
        None = 0,
        Control = 1,
        Shift = 2,
        Alt = 4
    }

    public CommandShortcut(Keys key)
    {
        Key = key;
    }

    public CommandShortcut(KeyModifiers mod, Keys key)
    {
        Modifier = mod;
        Key = key;
    }

    public KeyModifiers Modifier
    {
        get;
        private set;
    }

    public Keys Key
    {
        get;
        private set;
    }

    public string GetString()
    {
        // Key Shortcut
        var mod_str = Modifier switch
        {
            KeyModifiers.Control => "ModifierCtrl+",
            KeyModifiers.Shift => "ModifierShift+",
            KeyModifiers.Alt => "ModifierAlt+",
            KeyModifiers.Control | KeyModifiers.Shift => "ModifierCtrl+ModifierShift+",
            KeyModifiers.Control | KeyModifiers.Alt => "ModifierAlt+ModifierCtrl+",
            KeyModifiers.Alt | KeyModifiers.Shift => "ModifierAlt+ModifierShift+",
            KeyModifiers.Alt | KeyModifiers.Control | KeyModifiers.Shift => "ModifierAlt+ModifierCtrl+ModifierShift+",
            _ => ""
        };

        return $"{mod_str}{Key}";
    }

}