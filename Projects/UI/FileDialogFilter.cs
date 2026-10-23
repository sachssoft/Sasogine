using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// FileDialogFilter configuration and behavior.
/// </summary>
public sealed record FileDialogFilter(string Name, params string[] Extensions)
{
    internal bool Matches(string name)
    {
        if (Extensions.Length == 0)
            return true;
        foreach (string raw in Extensions)
        {
            string pattern = raw.Trim();
            if (pattern is "*" or "*.*")
                return true;
            string suffix = pattern.StartsWith('*') ? pattern[1..] : pattern.StartsWith('.') ? pattern : "." + pattern;
            if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
