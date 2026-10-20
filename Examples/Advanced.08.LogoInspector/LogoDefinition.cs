namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Observable configuration for the logo example; no member discovery is used.</summary>
internal sealed class LogoDefinition : Element, IEntityDefinition
{
    private string? _Id = "sasogine-logo";
    private static readonly PropertyChangedEventArgs IdArgs = new(nameof(Id));
    /// <summary>Gets or sets id.</summary>
    public string? Id { get => _Id; set => SetNotify(ref _Id, value, IdArgs); }

    private string? _Class = "Logo";
    private static readonly PropertyChangedEventArgs ClassArgs = new(nameof(Class));
    /// <summary>Gets or sets class.</summary>
    public string? Class { get => _Class; set => SetNotify(ref _Class, value, ClassArgs); }

    private string _Title = "SASOGINE";
    private static readonly PropertyChangedEventArgs TitleArgs = new(nameof(Title));
    /// <summary>Gets or sets title.</summary>
    public string Title { get => _Title; set => SetNotify(ref _Title, value ?? string.Empty, TitleArgs); }

    private string _Subtitle = "CREATE. PLAY. BUILD.";
    private static readonly PropertyChangedEventArgs SubtitleArgs = new(nameof(Subtitle));
    /// <summary>Gets or sets subtitle.</summary>
    public string Subtitle { get => _Subtitle; set => SetNotify(ref _Subtitle, value ?? string.Empty, SubtitleArgs); }

    private bool _Visible = true;
    private static readonly PropertyChangedEventArgs VisibleArgs = new(nameof(Visible));
    /// <summary>Gets or sets visible.</summary>
    public bool Visible { get => _Visible; set => SetNotify(ref _Visible, value, VisibleArgs); }
    public LogoTransformDefinition Transform { get; } = new();
    public LogoSpatialDefinition Spatial { get; } = new();
    public LogoAppearanceDefinition Appearance { get; } = new();
    public TrackableCollection<LogoAccentDefinition> Accents { get; } = new()
    {
        new()
        {
            Name = "Upper orbit",
            Angle = -35
        },
        new()
        {
            Name = "Lower orbit",
            Angle = 145,
            Length = 45
        }
    };
}
