namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Observable configuration for the logo example; no member discovery is used.</summary>
internal sealed class LogoAppearanceDefinition : Element, IDefinition
{
    private Reference<Sachssoft.Engine.Assets.Graphics.Texture2DAsset>? _previewTexture;
    private static readonly PropertyChangedEventArgs PreviewTextureArgs = new(nameof(PreviewTexture));
    /// <summary>Gets or sets the optional example texture reference.</summary>
    public Reference<Sachssoft.Engine.Assets.Graphics.Texture2DAsset>? PreviewTexture
    { get => _previewTexture; set => SetNotify(ref _previewTexture, value, PreviewTextureArgs); }

    private Color _Primary = new Color(94, 230, 246);
    private static readonly PropertyChangedEventArgs PrimaryArgs = new(nameof(Primary));
    /// <summary>Gets or sets primary.</summary>
    public Color Primary { get => _Primary; set => SetNotify(ref _Primary, value, PrimaryArgs); }

    private Color _Secondary = new Color(162, 143, 255);
    private static readonly PropertyChangedEventArgs SecondaryArgs = new(nameof(Secondary));
    /// <summary>Gets or sets secondary.</summary>
    public Color Secondary { get => _Secondary; set => SetNotify(ref _Secondary, value, SecondaryArgs); }

    private float _Stroke = 12;
    private static readonly PropertyChangedEventArgs StrokeArgs = new(nameof(Stroke));
    /// <summary>Gets or sets stroke.</summary>
    public float Stroke { get => _Stroke; set => SetNotify(ref _Stroke, Math.Clamp(value, 5, 22), StrokeArgs); }

    private bool _ShowFrame = true;
    private static readonly PropertyChangedEventArgs ShowFrameArgs = new(nameof(ShowFrame));
    /// <summary>Gets or sets showframe.</summary>
    public bool ShowFrame { get => _ShowFrame; set => SetNotify(ref _ShowFrame, value, ShowFrameArgs); }
}
