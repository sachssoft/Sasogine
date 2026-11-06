using Microsoft.Xna.Framework;
using Sachssoft.Engine.Scenes;

namespace Sachssoft.Engine;

/// <summary>Routes desktop window input without exposing desktop-only event types on the shared game base.</summary>
/// <remarks>Android MonoGame does not define InputKeyEventArgs or desktop keyboard window events.</remarks>
internal sealed class DesktopWindowInput
{
    /// <summary>Stores the application whose current scene receives window input.</summary>
    private readonly GameApplicationBase _application;

    /// <summary>Creates the desktop window input adapter.</summary>
    /// <param name="application">The desktop application whose window supplies input.</param>
    public DesktopWindowInput(GameApplicationBase application)
    {
        _application = application;
    }

    /// <summary>Attaches input handlers to the owning desktop window.</summary>
    public void Attach()
    {
        _application.Window.KeyDown += OnKeyDown;
        _application.Window.KeyUp += OnKeyUp;
        _application.Window.TextInput += OnTextInput;
    }

    /// <summary>Forwards a desktop key press to the active scene.</summary>
    /// <param name="sender">The window raising the event.</param>
    /// <param name="args">The pressed key.</param>
    private void OnKeyDown(object? sender, InputKeyEventArgs args)
    {
        if (_application.Scenes.CurrentScene is IClientKeyboardInput input)
            input.OnKeyDown(args.Key);
    }

    /// <summary>Forwards a desktop key release to the active scene.</summary>
    /// <param name="sender">The window raising the event.</param>
    /// <param name="args">The released key.</param>
    private void OnKeyUp(object? sender, InputKeyEventArgs args)
    {
        if (_application.Scenes.CurrentScene is IClientKeyboardInput input)
            input.OnKeyUp(args.Key);
    }

    /// <summary>Forwards a desktop text character to the active scene.</summary>
    /// <param name="sender">The window raising the event.</param>
    /// <param name="args">The entered character.</param>
    private void OnTextInput(object? sender, TextInputEventArgs args)
    {
        if (_application.Scenes.CurrentScene is IClientKeyboardInput input)
            input.OnTextInput(args.Character);
    }
}
