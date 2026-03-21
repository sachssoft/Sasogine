using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Surface.Controls;

public record FileSystemDialogFilter
{
    public string? Title { get; set; }
    public string? Pattern { get; set; }

    /// <summary>
    /// Wandelt einen Filterstring wie "Text files (*.txt)|*.txt|All files (*.*)|*.*" in FileChooseFilter-Objekte um.
    /// </summary>
    public static IEnumerable<FileSystemDialogFilter> FromString(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            yield break;

        var parts = filter.Split('|', StringSplitOptions.RemoveEmptyEntries);

        // immer paarweise: Titel | Pattern
        for (int i = 0; i + 1 < parts.Length; i += 2)
        {
            yield return new FileSystemDialogFilter
            {
                Title = parts[i].Trim(),
                Pattern = parts[i + 1].Trim()
            };
        }
    }
}
