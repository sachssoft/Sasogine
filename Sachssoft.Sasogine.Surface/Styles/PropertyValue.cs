using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Colors;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sachssoft.Sasogine.Surface.Styles
{
    public sealed class PropertyValue
    {
        private static readonly Dictionary<Type, Delegate> _converters = new();
        private readonly string? _rawValue;

        static PropertyValue()
        {
            // Ganzzahlige Typen
            RegisterConverter<sbyte>((raw, ctx) => sbyte.TryParse(raw, out var v) ? v : (sbyte)0);
            RegisterConverter<byte>((raw, ctx) => byte.TryParse(raw, out var v) ? v : (byte)0);
            RegisterConverter<short>((raw, ctx) => short.TryParse(raw, out var v) ? v : (short)0);
            RegisterConverter<ushort>((raw, ctx) => ushort.TryParse(raw, out var v) ? v : (ushort)0);
            RegisterConverter<int>((raw, ctx) => int.TryParse(raw, out var v) ? v : 0);
            RegisterConverter<uint>((raw, ctx) => uint.TryParse(raw, out var v) ? v : 0U);
            RegisterConverter<long>((raw, ctx) => long.TryParse(raw, out var v) ? v : 0L);
            RegisterConverter<ulong>((raw, ctx) => ulong.TryParse(raw, out var v) ? v : 0UL);

            // Gleitkommazahlen
            RegisterConverter<float>((raw, ctx) => float.TryParse(raw, NumberStyles.Float | NumberStyles.AllowThousands, UIEnvironment.Culture, out var v) ? v : 0f);
            RegisterConverter<double>((raw, ctx) => double.TryParse(raw, NumberStyles.Float | NumberStyles.AllowThousands, UIEnvironment.Culture, out var v) ? v : 0.0);
            RegisterConverter<decimal>((raw, ctx) => decimal.TryParse(raw, NumberStyles.Float | NumberStyles.AllowThousands, UIEnvironment.Culture, out var v) ? v : 0m);

            // Boolean & String
            RegisterConverter<bool>((raw, ctx) => bool.TryParse(raw, out var v) ? v : false);
            RegisterConverter<string>((raw, ctx) => raw ?? string.Empty);
            RegisterConverter<char>((raw, ctx) => string.IsNullOrEmpty(raw) ? '\0' : raw[0]);

            // Datum / Zeit
            RegisterConverter<DateTime>((raw, ctx) => DateTime.TryParse(raw, UIEnvironment.Culture, DateTimeStyles.None, out var v) ? v : default);
            RegisterConverter<TimeSpan>((raw, ctx) => TimeSpan.TryParse(raw, UIEnvironment.Culture, out var v) ? v : default);
            RegisterConverter<Guid>((raw, ctx) => Guid.TryParse(raw, out var v) ? v : Guid.Empty);

            RegisterConverter<Uri>((raw, ctx) => Uri.TryCreate(raw, UriKind.RelativeOrAbsolute, out var v) ? v : null);
            RegisterConverter<Version>((raw, ctx) => Version.TryParse(raw, out var v) ? v : null);
            RegisterConverter<CultureInfo>((raw, ctx) => TryParseCulture(raw, out var v) ? v : UIEnvironment.Culture);

            // Color & Thickness
            RegisterConverter<Color>((raw, ctx) => ValueConverterHelpers.TryParseColor(raw, out var v) ? v : new Color());
            RegisterConverter<Thickness>((raw, ctx) => Thickness.TryParse(raw, out var v) ? v : new Thickness());

            // MonoGame / XNA Structs
            RegisterConverter<Vector2>((raw, ctx) => TryParseVector2(raw, out var v) ? v : Vector2.Zero);
            RegisterConverter<Vector3>((raw, ctx) => TryParseVector3(raw, out var v) ? v : Vector3.Zero);
            RegisterConverter<Vector4>((raw, ctx) => TryParseVector4(raw, out var v) ? v : Vector4.Zero);
            RegisterConverter<Point>((raw, ctx) => TryParsePoint(raw, out var v) ? v : Point.Zero);
            RegisterConverter<Rectangle>((raw, ctx) => TryParseRectangle(raw, out var v) ? v : Rectangle.Empty);
            RegisterConverter<Matrix>((raw, ctx) => TryParseMatrix(raw, out var v) ? v : Matrix.Identity);
            RegisterConverter<Quaternion>((raw, ctx) => TryParseQuaternion(raw, out var v) ? v : Quaternion.Identity);

        }

        public PropertyValue(string? rawValue)
        {
            _rawValue = rawValue;
        }

        public string? RawValue => _rawValue;

        public static void RegisterConverter<T>(Func<string?, object?, T?> converter)
        {
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            _converters[typeof(T)] = converter;
        }

        public T? ConvertTo<T>(T? fallback = default)
        {
            if (_converters.TryGetValue(typeof(T), out var del) && del is Func<string?, object?, T?> typedDel)
            {
                return typedDel(_rawValue, null);
            }

            throw new InvalidOperationException($"No converter registered for type {typeof(T)} and auto-detection failed.");
        }

        public TEnum ConvertToEnum<TEnum>(TEnum fallback = default)
            where TEnum : struct, Enum
        {
            if (_rawValue != null && Enum.TryParse<TEnum>(_rawValue, ignoreCase: true, out var val))
                return val;

            return fallback;
        }

        #region 

        private static bool TryParseVector2(string? s, out Vector2 result)
        {
            result = Vector2.Zero;
            if (string.IsNullOrEmpty(s)) return false;
            var parts = s.Split(',');
            if (parts.Length != 2) return false;
            return float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out result.X)
                && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out result.Y);
        }

        private static bool TryParseVector3(string? s, out Vector3 result)
        {
            result = Vector3.Zero;
            if (string.IsNullOrEmpty(s)) return false;
            var parts = s.Split(',');
            if (parts.Length != 3) return false;
            return float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out result.X)
                && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out result.Y)
                && float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out result.Z);
        }

        private static bool TryParseVector4(string? s, out Vector4 result)
        {
            result = Vector4.Zero;
            if (string.IsNullOrEmpty(s)) return false;
            var parts = s.Split(',');
            if (parts.Length != 4) return false;
            return float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out result.X)
                && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out result.Y)
                && float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out result.Z)
                && float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out result.W);
        }

        private static bool TryParsePoint(string? s, out Point result)
        {
            result = Point.Zero;
            if (string.IsNullOrEmpty(s)) return false;
            var parts = s.Split(',');
            if (parts.Length != 2) return false;
            return int.TryParse(parts[0], out result.X)
                && int.TryParse(parts[1], out result.Y);
        }

        private static bool TryParseRectangle(string? s, out Rectangle result)
        {
            result = Rectangle.Empty;
            if (string.IsNullOrEmpty(s)) return false;
            var parts = s.Split(',');
            if (parts.Length != 4) return false;
            return int.TryParse(parts[0], out result.X)
                && int.TryParse(parts[1], out result.Y)
                && int.TryParse(parts[2], out result.Width)
                && int.TryParse(parts[3], out result.Height);
        }

        private static bool TryParseMatrix(string? s, out Matrix result)
        {
            result = Matrix.Identity;
            if (string.IsNullOrEmpty(s)) return false;
            var parts = s.Split(',');
            if (parts.Length != 16) return false;
            float[] values = new float[16];
            for (int i = 0; i < 16; i++)
            {
                if (!float.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out values[i]))
                    return false;
            }
            result = new Matrix(
                values[0], values[1], values[2], values[3],
                values[4], values[5], values[6], values[7],
                values[8], values[9], values[10], values[11],
                values[12], values[13], values[14], values[15]);
            return true;
        }

        private static bool TryParseQuaternion(string? s, out Quaternion result)
        {
            result = Quaternion.Identity;
            if (string.IsNullOrEmpty(s)) return false;
            var parts = s.Split(',');
            if (parts.Length != 4) return false;
            return float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out result.X)
                && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out result.Y)
                && float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out result.Z)
                && float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out result.W);
        }

        private static bool TryParseCulture(string? cultureName, out CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(cultureName))
            {
                culture = CultureInfo.InvariantCulture;
                return false;
            }

            try
            {
                culture = CultureInfo.GetCultureInfo(cultureName);
                return true;
            }
            catch (CultureNotFoundException)
            {
                culture = CultureInfo.InvariantCulture;
                return false;
            }
        }

        #endregion
    }
}
