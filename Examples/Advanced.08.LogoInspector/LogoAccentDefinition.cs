namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Observable configuration for the logo example; no member discovery is used.</summary>
internal sealed class LogoAccentDefinition : Element, IDefinition
{
    private string _Name = "Accent";
    private static readonly PropertyChangedEventArgs NameArgs = new(nameof(Name));
    /// <summary>Gets or sets name.</summary>
    public string Name { get => _Name; set => SetNotify(ref _Name, value ?? string.Empty, NameArgs); }

    private float _Angle = 30;
    private static readonly PropertyChangedEventArgs AngleArgs = new(nameof(Angle));
    /// <summary>Gets or sets angle.</summary>
    public float Angle { get => _Angle; set => SetNotify(ref _Angle, Math.Clamp(value, -180, 180), AngleArgs); }

    private float _Length = 30;
    private static readonly PropertyChangedEventArgs LengthArgs = new(nameof(Length));
    /// <summary>Gets or sets length.</summary>
    public float Length { get => _Length; set => SetNotify(ref _Length, Math.Clamp(value, 8, 75), LengthArgs); }

    private bool _Visible = true;
    private static readonly PropertyChangedEventArgs VisibleArgs = new(nameof(Visible));
    /// <summary>Gets or sets visible.</summary>
    public bool Visible { get => _Visible; set => SetNotify(ref _Visible, value, VisibleArgs); }
}
