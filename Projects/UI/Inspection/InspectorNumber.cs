using System.Globalization;
using System.Numerics;

namespace Sachssoft.Engine.Controls.Inspection;
/// <summary>Constructs numeric text fields without converting through float, decimal or object.</summary>
internal static class InspectorNumber
{
    internal static TypedValueTextBox<TNumber> Create<TNumber>(TNumber value)
        where TNumber : struct, INumber<TNumber> => new(value, static number => number.ToString(null, CultureInfo.InvariantCulture), static (string text, out TNumber number) => TNumber.TryParse(text, CultureInfo.InvariantCulture, out number) && TNumber.IsFinite(number));
}
