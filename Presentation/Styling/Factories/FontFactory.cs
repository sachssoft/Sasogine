using Sachssoft.Sasogine.Presentation.Rendering;
using Sachssoft.Sasogine.Resources;
using System;

namespace Sachssoft.Sasogine.Presentation.Styling.Factories;

internal class FontFactory : ITypeFactory<Font, Resource>
{
    public Font Create(ResourceStore store, Resource entry)
    {
        string? name = null;
        FontWeight weight = FontWeight.Normal;
        FontStyle style = FontStyle.Normal;
        int size = 16;

        foreach (var property in entry.Properties)
        {
            switch (property.Name)
            {
                case nameof(Font.Name):
                    if (property.Value is string fn && !string.IsNullOrWhiteSpace(fn))
                        name = fn;
                    break;

                case nameof(Font.Weight):
                    if (property.Value is FontWeight fw)
                        weight = fw;
                    break;

                case nameof(Font.Style):
                    if (property.Value is FontStyle fs)
                        style = fs;
                    break;

                case nameof(Font.Size):
                    if (property.Value is int fsz && fsz > 0)
                        size = fsz;
                    break;
            }
        }

        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException($"Font '{nameof(Font.Name)}' must be provided in Resource '{entry.Id}'.");

        return new Font
        {
            Name = name,
            Size = size,
            Style = style,
            Weight = weight
        };
    }
}