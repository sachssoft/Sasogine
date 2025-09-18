using Sachssoft.Observables.Descriptors;

using System;

/// <summary>
/// Konvertiert zwischen Radians (intern) und Degrees (UI-Anzeige).
/// </summary>
public class RadianDegreeConverter : IDisplayValueConverter
{
    /// <summary>
    /// Konvertiert internen Wert (Radians) zur Anzeige (Degrees).
    /// </summary>
    public object ConvertToDisplay(object value)
    {
        if (value is float radians)
            return radians * (180f / MathF.PI);
        return value;
    }

    /// <summary>
    /// Konvertiert Wert aus UI (Degrees) zurück zu internem Wert (Radians).
    /// </summary>
    public object ConvertFromDisplay(object value)
    {
        if (value is float degrees)
            return degrees * (MathF.PI / 180f);
        return value;
    }

    public bool CanConvert(Type targetType)
    {
        return targetType == typeof(float);
    }
}