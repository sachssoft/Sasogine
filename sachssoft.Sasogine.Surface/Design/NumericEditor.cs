using sachssoft.Sasogine.Surface.Design;
using sachssoft.Sasogine.Surface.Visuals.Controls;
using System.Globalization;
using System;

public class NumericEditor : PropertyEditorBase
{
    public NumericEditor()
    {
    }

    public NumericEditor(float min, float max)
    {
    }

    public override Widget CreateControl<T>(
        Action<T, string> changed,
        Func<T, object?> getter,
        Action<T, object?> setter)
    {

        var spin = new SpinButton
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            //Integer = typeCode is >= TypeCode.SByte and <= TypeCode.UInt64,
            //Nullable = isNullable,
            DecimalPlaces = 3,
            Increment = 1
        };

        //var value = getter(source);
        //spin.Value = value != null ? Convert.ToSingle(value, culture) : null;

        //spin.ValueChanged += (s, e) =>
        //{
        //    if (spin.Value is null)
        //    {
        //        setter(source, null);
        //        changed(source, "");
        //        return;
        //    }

        //    var val = spin.Value.Value;
        //    object converted = typeCode switch
        //    {
        //        TypeCode.SByte => Convert.ToSByte(val, culture),
        //        TypeCode.Byte => Convert.ToByte(val, culture),
        //        TypeCode.Int16 => Convert.ToInt16(val, culture),
        //        TypeCode.UInt16 => Convert.ToUInt16(val, culture),
        //        TypeCode.Int32 => Convert.ToInt32(val, culture),
        //        TypeCode.UInt32 => Convert.ToUInt32(val, culture),
        //        TypeCode.Int64 => Convert.ToInt64(val, culture),
        //        TypeCode.UInt64 => Convert.ToUInt64(val, culture),
        //        TypeCode.Single => Convert.ToSingle(val, culture),
        //        TypeCode.Double => Convert.ToDouble(val, culture),
        //        TypeCode.Decimal => Convert.ToDecimal(val, culture),
        //        _ => throw new NotSupportedException("Nicht unterstützter Zahlentyp: " + propType)
        //    };

        //    setter(source, converted);
        //    changed(source, "");
        //};

        return spin;
    }
}
