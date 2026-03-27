using Sachssoft.Sasofly.Inspection.Descriptors;
using System;
using System.Linq;

namespace Sachssoft.Sasogine.Surface.Controls.Inspectors
{
    public class NumericValueEditor : InspectorValueEditorBase
    {
        private IDisplayValueConverter? _displayValueConverter;
        private NumberSpinner? _spinButton;

        protected internal override Widget BuildControl()
        {
            var metadata = Property.GetMetadata(Source.GetType());

            var desc1 = metadata.Descriptors
                .OfType<DisplayValueConverterDescriptor>()
                .FirstOrDefault();

            var desc2 = metadata.Descriptors
                .OfType<RangeValueDescriptor>()
                .FirstOrDefault();

            var desc3 = metadata.Descriptors
                .OfType<RoundValueDescriptor>()
                .FirstOrDefault();

            if (desc1 != null)
                _displayValueConverter = desc1.Converter;

            var numericType = Property.ValueType;

            //if (numericType.IsNullablePrimitive())
            //    numericType = numericType.GetNullableType();

            _spinButton = new NumberSpinner
            {
                //Integer = numericType.IsNumericInteger(),
                //Nullable = Property.ValueType.IsNullablePrimitive()
            };

            if (desc2 != null)
            {
                //_spinButton.Minimum = (float)desc2.ParseMinimum(typeof(float), Inspector.Culture);
                //_spinButton.Maximum = (float)desc2.ParseMaximum(typeof(float), Inspector.Culture);
            }

            if (desc3 != null)
            {
                //_spinButton.DecimalPlaces = desc3.DecimalPlaces;
                //_spinButton.SmallIncrement = desc3.SmallIncrement;
                //_spinButton.LargeIncrement = desc3.LargeIncrement;
            }

            // Initialwert setzen
            EnsureGetValue();

            if (!Property.IsReadOnly)
            {
                //_spinButton.ValueChanged += (s, e) =>
                //{
                //    SaveValue(_spinButton, numericType);
                //};
                _spinButton.KeyDown += (s, e) =>
                {
                    if (e.Data == Microsoft.Xna.Framework.Input.Keys.Enter)
                    {
                        SaveValue(_spinButton, numericType);
                    }
                };
                _spinButton.KeyboardFocusLost += (s, e) =>
                {
                    SaveValue(_spinButton, numericType);
                };
            }
            else
            {
                _spinButton.IsEnabled = false;
            }

            return _spinButton;
        }

        private void EnsureGetValue()
        {            
            // Initialwert setzen
            var value = GetValue();
            //_spinButton.Value = value != null ? GetDisplayValue(value) : default(float?);
        }

        protected override void OnValueChangedBySource()
        {
            EnsureGetValue();
        }

        private float GetDisplayValue(object value)
        {
            if (_displayValueConverter == null || !_displayValueConverter.CanConvert(typeof(float)))
                return Convert.ToSingle(value);

            var floatValue = Convert.ToSingle(value);
            return (float)_displayValueConverter.ConvertToDisplay(floatValue);
        }

        private void SaveValue(NumberSpinner spinButton, Type numericType)
        {
            try
            {
                object? result = null;

                //if (spinButton.Value.HasValue)
                //{
                //    float valueToConvert = spinButton.Value.Value;

                //    // Falls ein DisplayValueConverter vorhanden ist, konvertiere zurück
                //    if (_displayValueConverter != null && _displayValueConverter.CanConvert(typeof(float)))
                //    {
                //        valueToConvert = (float)_displayValueConverter.ConvertFromDisplay(valueToConvert);
                //    }

                //    // Nur erlaubte numerische Typen konvertieren
                //    if (IsNumericPrimitive(numericType) || numericType.IsEnum)
                //    {
                //        result = Convert.ChangeType(valueToConvert, numericType);
                //    }
                //    else
                //    {
                //        throw new InvalidCastException($"Cannot convert float to {numericType.Name}");
                //    }
                //}

                SetValue(result);
            }
            catch (Exception ex)
            {
                var value = GetValue();
                // Alte Value wiederherstellen
                //spinButton.Value = value != null ? GetDisplayValue(value) : default(float?);
                // Optional: Logging oder Dialog
                Console.WriteLine($"Invalid _value: {ex}");
            }
        }

        private static bool IsNumericPrimitive(Type type)
        {
            if (type.IsEnum) return false;

            TypeCode typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
                default:
                    return false;
            }
        }

    }
}
