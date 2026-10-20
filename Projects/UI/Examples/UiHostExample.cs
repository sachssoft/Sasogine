using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Sachssoft.Engine.Graphics.Text;
using System;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Shows application integration without selecting a graphics backend.</summary>
public sealed class UiHostExample : IDisposable
{
    private readonly GameApplicationBase _application;
    private readonly FontFaceControlFont _font;
    private readonly ControlLayoutContext _layout;
    private readonly ControlDrawContext _draw;
    private readonly ControlInputSampler _sampler = new();
    private bool _ready, _disposed;
    private readonly Button _button;
    private readonly TextBlock _status;
    private int _activationCount;
    private readonly ControlSkin? _skin;
    /// <summary>Creates a small interface after the application's graphics device is initialized.</summary>
    public UiHostExample(GameApplicationBase application, FontFace face, string? skinDirectory = null)
    {
        _application = application ?? throw new ArgumentNullException(nameof(application));
        _font = new FontFaceControlFont(face, 20);
        _layout = new ControlLayoutContext(_font);
        _draw = new ControlDrawContext(application, _font, application.GraphicsDevice.Viewport.Bounds);
        var page = new Panel
        {
            StyleId = "Page"
        };
        Workspace = new Workspace(page);
        var registry = Workspace.Registry;
        registry.Styles.Register("Page", new Style { BasedOn = "Panel", Background = new Color(18, 22, 30) });
        registry.Styles.Register("Menu", new Style { BasedOn = "StackPanel", Width = 360, Margin = new Insets2(28), Padding = new Insets2(20), Spacing = 14, HorizontalAlignment = ControlAlignment.Start, VerticalAlignment = ControlAlignment.Start, Background = new Color(30, 36, 48) });
        registry.Styles.Register("Heading", new Style { BasedOn = "Label", TextColor = new Color(120, 190, 255) });
        registry.Styles.Register("PrimaryButton", new Style { BasedOn = "Button", Height = 46 });
        var menu = new StackPanel
        {
            StyleId = "Menu"
        };
        var heading = new Label
        {
            StyleId = "Heading",
            Text = "Sasogine UI"
        };
        _status = new TextBlock
        {
            Text = "Ready."
        };
        _button = new Button
        {
            Content = new TextBlock
            {
                Text = "Click or press Space"
            },
            StyleId = "PrimaryButton"
        };
        menu.Add(heading);
        menu.Add(_status);
        menu.Add(_button);
        page.Add(menu);
        if (skinDirectory is not null)
        {
            _skin = ControlSkin.Load(application.GraphicsDevice, skinDirectory);
            Workspace.SetSkin(_skin);
        }
    }

    /// <summary>Gets the example tree for inspection and customization.</summary>
    public Workspace Workspace { get; }

    /// <summary>Forwards a native text event on the UI thread.</summary>
    public void OnTextInput(char character) => _sampler.AddText(character);
    /// <summary>Processes caller-sampled devices once per update.</summary>
    public void Update(GameTime time, MouseState mouse, KeyboardState keyboard, bool active, float scale = 1) => Update(time, mouse, keyboard, default(TouchCollection), active, scale);
    /// <summary>Processes caller-sampled mouse, touch and keyboard snapshots.</summary>
    public void Update(GameTime time, MouseState mouse, KeyboardState keyboard, TouchCollection touches, bool active, float scale = 1)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Rectangle viewport = _application.GraphicsDevice.Viewport.Bounds;
        _ready = viewport.Width > 0 && viewport.Height > 0;
        if (!_ready)
            return;
        _draw.SetViewport(viewport, scale);
        Workspace.Update(_sampler.SampleReusable(mouse, keyboard, touches, viewport, scale, time.ElapsedGameTime, active), _layout);
        if (_button.IsPressed)
            _status.Text = $"Activated {++_activationCount} time(s).";
    }

    /// <summary>Draws after scene rendering has ended its batches.</summary>
    public void Draw(GameTime time)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_ready)
            return;
        _draw.SetFrameTime(time);
        Workspace.Draw(_draw);
    }

    /// <summary>Disposes UI-owned resources, preserving the source FontFace.</summary>
    public void Dispose()
    {
        if (_disposed)
            return;
        Workspace.Dispose();
        _draw.Dispose();
        _skin?.Dispose();
        _font.Dispose();
        _disposed = true;
    }
}
