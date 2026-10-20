using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sachssoft.Engine.Controls.Styling;
internal sealed class StyleValueConverter<T>(Func<string, T> parse, Func<T, string>? format) : IStyleValueConverter
{
    internal T Parse(string text) => parse(text);
    internal string Format(T value) => format is null ? throw new NotSupportedException("This converter supports parsing only.") : format(value);
}
