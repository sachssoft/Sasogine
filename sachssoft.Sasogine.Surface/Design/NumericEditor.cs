using sachssoft.Sasogine.Surface.Design;
using sachssoft.Sasogine.Surface.Visuals.Controls;
using System;
using System.Globalization;

public class NumericEditor : PropertyEditorBase
{
    private SpinButton _spin_button;
    private float _min = float.MinValue;
    private float _max = float.MaxValue;
    private int _decimal_places = 3;
    private float _increment = 1;

    public NumericEditor() { }

    public NumericEditor(float min, float max, int decimal_places = 3, float increment = 1)
    {
        _min = min;
        _max = max;
        _decimal_places = decimal_places;
        _increment = increment;
    }

    public override Widget CreateControl<T>(
        Action<T, string> changed,
        Func<T, object?> getter,
        Action<T, object?> setter)
    {
        _spin_button = new SpinButton
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            DecimalPlaces = _decimal_places,
            Increment = _increment,
            Minimum = _min,
            Maximum = _max
        };

        var value = getter.Invoke((T)Source);
        float? float_value = TryConvertToFloat(value);

        if (float_value.HasValue)
            _spin_button.Value = float_value.Value;

        _spin_button.ValueChanged += (s, e) =>
        {
            var float_value = _spin_button.Value;

            // Keine Änderung, wenn leer (z. B. durch Benutzereingabe)
            if (float_value == null)
            {
                setter.Invoke((T)Source, null);
                changed.Invoke((T)Source, string.Empty);
                return;
            }

            var current_value = getter.Invoke((T)Source);
            var target_type = current_value?.GetType() ?? typeof(float);

            object? converted = ConvertToTargetType(float_value.Value, target_type);
            setter.Invoke((T)Source, converted);
            changed.Invoke((T)Source, string.Empty);
        };

        return _spin_button;
    }

    private static float? TryConvertToFloat(object? value)
    {
        if (value == null)
            return null;

        try
        {
            return Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    private static object? ConvertToTargetType(float value, Type target_type)
    {
        if (Nullable.GetUnderlyingType(target_type) is Type underlying_type)
            target_type = underlying_type;

        try
        {
            return Type.GetTypeCode(target_type) switch
            {
                TypeCode.SByte => Convert.ToSByte(value),
                TypeCode.Byte => Convert.ToByte(value),
                TypeCode.Int16 => Convert.ToInt16(value),
                TypeCode.UInt16 => Convert.ToUInt16(value),
                TypeCode.Int32 => Convert.ToInt32(value),
                TypeCode.UInt32 => Convert.ToUInt32(value),
                TypeCode.Int64 => Convert.ToInt64(value),
                TypeCode.UInt64 => Convert.ToUInt64(value),
                TypeCode.Single => Convert.ToSingle(value),
                TypeCode.Double => Convert.ToDouble(value),
                TypeCode.Decimal => Convert.ToDecimal(value),
                _ => value
            };
        }
        catch
        {
            return value;
        }
    }
}
