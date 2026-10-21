using Microsoft.Xna.Framework;
using Sachssoft.Engine.Gameplay.Curves;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sachssoft.Engine.Controls.Styling;
/// <summary>Explicit named converters. Generic delegate registration is AOT-safe and requires no reflection.</summary>
public sealed class ValueConverterRegistry
{
    private int _revision;
    internal int Revision => _revision;

    private readonly Dictionary<string, IStyleValueConverter> _converters = new(StringComparer.Ordinal);
    /// <summary>Creates the built-in explicit converters.</summary>
    public ValueConverterRegistry()
    {
        Register<AssetViewMode>("AssetViewMode", static s => s switch
        {
            "Tiles" => AssetViewMode.Tiles,
            "List" => AssetViewMode.List,
            _ => throw new FormatException("Unknown AssetViewMode.")}, static v => v == AssetViewMode.Tiles ? "Tiles" : "List");
        Register<ChartKind>("ChartKind", static s => s switch
        {
            "Line" => ChartKind.Line,
            "Bar" => ChartKind.Bar,
            "Step" => ChartKind.Step,
            _ => throw new FormatException("Unknown ChartKind.")}, static v => v switch
        {
            ChartKind.Line => "Line",
            ChartKind.Bar => "Bar",
            ChartKind.Step => "Step",
            _ => throw new FormatException("Unknown ChartKind.")});
        Register<ScriptLanguage>("ScriptLanguage", static s => s switch
        {
            "PlainText" => ScriptLanguage.PlainText,
            "Lua" => ScriptLanguage.Lua,
            "Python" => ScriptLanguage.Python,
            "CSharp" => ScriptLanguage.CSharp,
            _ => throw new FormatException("Unknown ScriptLanguage.")}, static v => v switch
        {
            ScriptLanguage.PlainText => "PlainText",
            ScriptLanguage.Lua => "Lua",
            ScriptLanguage.Python => "Python",
            ScriptLanguage.CSharp => "CSharp",
            _ => throw new FormatException("Unknown ScriptLanguage.")});
        Register<CurveInterpolation?>("NullableCurveInterpolation", static s => s switch
        {
            "null" => null,
            "Step" => CurveInterpolation.Step,
            "Linear" => CurveInterpolation.Linear,
            "Monotone" => CurveInterpolation.Monotone,
            _ => throw new FormatException("Unknown curve interpolation.")}, static v => v switch
        {
            null => "null",
            CurveInterpolation.Step => "Step",
            CurveInterpolation.Linear => "Linear",
            CurveInterpolation.Monotone => "Monotone",
            _ => throw new FormatException("Unknown curve interpolation.")});
        Register<decimal>("Decimal", static s => decimal.Parse(s, NumberStyles.Number, CultureInfo.InvariantCulture), static v => v.ToString(CultureInfo.InvariantCulture));
        Register<Control?>("Control", static s => Null(s) ? null : throw new KeyNotFoundException("Register a Control converter for named content."), static v => v is null ? "null" : throw new NotSupportedException("Provide a content ID or typed control."));
        Register<DateOnly>("Date", static s => DateOnly.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture), static d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        Register<DateOnly?>("NullableDate", static s => Null(s) ? null : DateOnly.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture), static d => d?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "null");
        Register<DayOfWeek>("DayOfWeek", static s => s switch
        {
            "Sunday" => DayOfWeek.Sunday,
            "Monday" => DayOfWeek.Monday,
            "Tuesday" => DayOfWeek.Tuesday,
            "Wednesday" => DayOfWeek.Wednesday,
            "Thursday" => DayOfWeek.Thursday,
            "Friday" => DayOfWeek.Friday,
            "Saturday" => DayOfWeek.Saturday,
            _ => throw new FormatException("Unknown weekday.")}, static d => d switch
        {
            DayOfWeek.Sunday => "Sunday",
            DayOfWeek.Monday => "Monday",
            DayOfWeek.Tuesday => "Tuesday",
            DayOfWeek.Wednesday => "Wednesday",
            DayOfWeek.Thursday => "Thursday",
            DayOfWeek.Friday => "Friday",
            DayOfWeek.Saturday => "Saturday",
            _ => throw new FormatException("Unknown weekday.")});
        Register<JoystickMovementMode>("JoystickMovementMode", static s => s switch
        {
            "Free" => JoystickMovementMode.Free,
            "Horizontal" => JoystickMovementMode.Horizontal,
            "Vertical" => JoystickMovementMode.Vertical,
            "CustomAxis" => JoystickMovementMode.CustomAxis,
            _ => throw InvalidEnum(s)}, static v => v switch
        {
            JoystickMovementMode.Free => "Free",
            JoystickMovementMode.Horizontal => "Horizontal",
            JoystickMovementMode.Vertical => "Vertical",
            JoystickMovementMode.CustomAxis => "CustomAxis",
            _ => throw InvalidEnum("JoystickMovementMode")});
        Register<JoystickPlacementMode>("JoystickPlacementMode", static s => s switch
        {
            "Fixed" => JoystickPlacementMode.Fixed,
            "Floating" => JoystickPlacementMode.Floating,
            _ => throw InvalidEnum(s)}, static v => v switch
        {
            JoystickPlacementMode.Fixed => "Fixed",
            JoystickPlacementMode.Floating => "Floating",
            _ => throw InvalidEnum("JoystickPlacementMode")});
        Register<Vector2>("Vector2", static s =>
        {
            float[] a = Numbers(s);
            return a.Length == 2 ? new Vector2(a[0], a[1]) : throw new FormatException("Vector requires two numbers.");
        }, static v => Join(v.X, v.Y));
        Register<CurveBase?>("Curve", static s => s switch
        {
            "null" => null,
            "Linear" => new LinearCurve(),
            "QuadraticIn" => new QuadraticInCurve(),
            "CubicIn" => new CubicInCurve(),
            "SineInOut" => new SineInOutCurve(),
            _ => throw new KeyNotFoundException("Register a Curve converter for application curve IDs.")}, static c => c switch
        {
            null => "null",
            LinearCurve => "Linear",
            QuadraticInCurve => "QuadraticIn",
            CubicInCurve => "CubicIn",
            SineInOutCurve => "SineInOut",
            _ => throw new NotSupportedException("Use a named curve ID in a style.")});
        Register<System.Windows.Input.ICommand?>("Command", static text => Null(text) ? null : throw new KeyNotFoundException("Register a Command converter for named application commands."), static command => command is null ? "null" : throw new NotSupportedException("Use a named command ID in a style."));
        Register<object?>("CommandParameter", static text => text == "null" ? null : text, static value => value is null ? "null" : value is string text ? text : throw new NotSupportedException("Provide object command parameters in the typed definition."));
        Register<ColorPickerModel>("ColorPickerModel", static value => value switch
        {
            "Rgba" => ColorPickerModel.Rgba,
            "Rgb" => ColorPickerModel.Rgb,
            "NormalizedRgb" => ColorPickerModel.NormalizedRgb,
            "Hsl" => ColorPickerModel.Hsl,
            "Hsv" => ColorPickerModel.Hsv,
            "LinearRgb" => ColorPickerModel.LinearRgb,
            "Oklab" => ColorPickerModel.Oklab,
            _ => throw new FormatException("Unknown color model.")}, static value => value switch
        {
            ColorPickerModel.Rgba => "Rgba",
            ColorPickerModel.Rgb => "Rgb",
            ColorPickerModel.NormalizedRgb => "NormalizedRgb",
            ColorPickerModel.Hsl => "Hsl",
            ColorPickerModel.Hsv => "Hsv",
            ColorPickerModel.LinearRgb => "LinearRgb",
            ColorPickerModel.Oklab => "Oklab",
            _ => throw new FormatException("Unknown color model.")});
        Register("GridLength", GridLength.Parse, static value => value.ToString());
        Register("String", static s => s, static s => s);
        Register("Boolean", static s => bool.Parse(s), static v => v ? "true" : "false");
        Register<bool?>("NullableBoolean", static s => Null(s) ? null : bool.Parse(s), static v => v.HasValue ? (v.Value ? "true" : "false") : "null");
        Register("Int32", static s => int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture), static v => v.ToString(CultureInfo.InvariantCulture));
        Register("Single", Number, static v => v.ToString("R", CultureInfo.InvariantCulture));
        Register<float?>("NullableSingle", static s => Null(s) ? null : Number(s), static v => v?.ToString("R", CultureInfo.InvariantCulture) ?? "null");
        Register("Insets2", static s =>
        {
            float[] a = Numbers(s);
            return a.Length switch
            {
                1 => new Insets2(a[0]),
                2 => new Insets2(a[0], a[1]),
                4 => new Insets2(a[0], a[1], a[2], a[3]),
                _ => throw new FormatException("Insets require one, two or four numbers.")};
        }, static v => Join(v.Left, v.Top, v.Right, v.Bottom));
        Register("Size2", static s =>
        {
            float[] a = Numbers(s);
            return a.Length switch
            {
                1 => new Size2(a[0]),
                2 => new Size2(a[0], a[1]),
                _ => throw new FormatException("Size requires one or two numbers.")};
        }, static v => Join(v.Width, v.Height));
        Register<Vector2?>("NullableVector2", static s =>
        {
            if (Null(s))
                return null;
            float[] a = Numbers(s);
            return a.Length == 2 ? new Vector2(a[0], a[1]) : throw new FormatException("Vector requires two numbers.");
        }, static v => v.HasValue ? Join(v.Value.X, v.Value.Y) : "null");
        Register("Color", ParseColor, static v => $"{v.R},{v.G},{v.B},{v.A}");
        Register<ControlAlignment>("ControlAlignment", static s => s switch
        {
            "Start" => ControlAlignment.Start,
            "Center" => ControlAlignment.Center,
            "End" => ControlAlignment.End,
            "Stretch" => ControlAlignment.Stretch,
            _ => throw InvalidEnum(s)}, static v => v switch
        {
            ControlAlignment.Start => "Start",
            ControlAlignment.Center => "Center",
            ControlAlignment.End => "End",
            ControlAlignment.Stretch => "Stretch",
            _ => throw InvalidEnum("ControlAlignment")});
        Register<StackOrientation>("StackOrientation", static s => s switch
        {
            "Horizontal" => StackOrientation.Horizontal,
            "Vertical" => StackOrientation.Vertical,
            _ => throw InvalidEnum(s)}, static v => v == StackOrientation.Horizontal ? "Horizontal" : v == StackOrientation.Vertical ? "Vertical" : throw InvalidEnum("StackOrientation"));
        Register<ControlOrientation>("ControlOrientation", static s => s switch
        {
            "Horizontal" => ControlOrientation.Horizontal,
            "Vertical" => ControlOrientation.Vertical,
            _ => throw InvalidEnum(s)}, static v => v == ControlOrientation.Horizontal ? "Horizontal" : v == ControlOrientation.Vertical ? "Vertical" : throw InvalidEnum("ControlOrientation"));
        Register<ScrollBarVisibility>("ScrollBarVisibility", static s => s switch
        {
            "Auto" => ScrollBarVisibility.Auto,
            "Always" => ScrollBarVisibility.Always,
            "Hidden" => ScrollBarVisibility.Hidden,
            _ => throw InvalidEnum(s)}, static v => v switch
        {
            ScrollBarVisibility.Auto => "Auto",
            ScrollBarVisibility.Always => "Always",
            ScrollBarVisibility.Hidden => "Hidden",
            _ => throw InvalidEnum("ScrollBarVisibility")});
        Register<IControlFont?>("Font", static s => Null(s) ? null : throw new KeyNotFoundException("Register the Font converter for named application fonts."), static v => v is null ? "null" : throw new NotSupportedException("Use a named font ID in the style."));
        Register<ImageStretch>("ImageStretch", static s => s switch
        {
            "None" => ImageStretch.None,
            "Fill" => ImageStretch.Fill,
            "Uniform" => ImageStretch.Uniform,
            "UniformToFill" => ImageStretch.UniformToFill,
            _ => throw InvalidEnum(s)}, static v => v switch
        {
            ImageStretch.None => "None",
            ImageStretch.Fill => "Fill",
            ImageStretch.Uniform => "Uniform",
            ImageStretch.UniformToFill => "UniformToFill",
            _ => throw InvalidEnum("ImageStretch")});
    }

    /// <summary>Registers or replaces one named converter. A formatter is optional.</summary>
    public void Register<T>(string id, Func<string, T> parse, Func<T, string>? format = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(parse);
        _converters[id] = new StyleValueConverter<T>(parse, format);
        unchecked
        {
            _revision++;
        }
    }

    /// <summary>Converts through a compile-time-known delegate and validates the registered value type.</summary>
    public T Convert<T>(string id, string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return _converters[id] is StyleValueConverter<T> converter ? converter.Parse(text) : throw new InvalidOperationException($"Converter '{id}' has a different registered value type.");
    }

    /// <summary>Formats a typed value through an explicitly registered formatter.</summary>
    public string Format<T>(string id, T value) => _converters[id] is StyleValueConverter<T> converter ? converter.Format(value) : throw new InvalidOperationException($"Converter '{id}' has a different registered value type.");
    private static bool Null(string s) => s.Length == 0 || s == "null";
    private static FormatException InvalidEnum(string s) => new($"Unknown enum value '{s}'.");
    private static float Number(string s)
    {
        float v = float.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);
        return float.IsFinite(v) ? v : throw new FormatException("Expected a finite number.");
    }

    private static float[] Numbers(string s)
    {
        string[] parts = s.Split(',');
        var a = new float[parts.Length];
        for (int i = 0; i < a.Length; i++)
            a[i] = Number(parts[i]);
        return a;
    }

    private static string Join(params float[] values)
    {
        var parts = new string[values.Length];
        for (int i = 0; i < parts.Length; i++)
            parts[i] = values[i].ToString("R", CultureInfo.InvariantCulture);
        return string.Join(",", parts);
    }

    private static Color ParseColor(string s)
    {
        if (s == "Transparent")
            return Color.Transparent;
        if (!s.Contains(','))
            return ControlSkin.ParseColor(s);
        string[] p = s.Split(',');
        if (p.Length is not (3 or 4))
            throw new FormatException("Color requires RGB or RGBA.");
        byte B(int i) => byte.Parse(p[i], NumberStyles.Integer, CultureInfo.InvariantCulture);
        return new Color(B(0), B(1), B(2), p.Length == 4 ? B(3) : (byte)255);
    }
}
