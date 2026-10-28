using Microsoft.Xna.Framework;
using Sachssoft.Engine.Services.Platform;
using System;

namespace Sachssoft.Engine.Platform.iOS;

/// <summary>Reports the mobile window capabilities of iOS without emulating desktop window states.</summary>
public sealed class iOSWindow : IPlatformWindow
{
    /// <summary>Stores the game whose title can be inspected.</summary>
    private readonly Game _game;

    /// <summary>Creates the mobile window capability service.</summary>
    /// <param name="game">The associated game.</param>
    /// <exception cref="ArgumentNullException">The game is null.</exception>
    public iOSWindow(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        _game = game;
    }

    /// <inheritdoc/>
    public bool SupportsWindowMode => false;

    /// <inheritdoc/>
    public bool SupportsTitle => false;

    /// <inheritdoc/>
    public bool SupportsCloseControl => false;

    /// <inheritdoc/>
    public bool SupportsResize => false;

    /// <inheritdoc/>
    public bool SupportsMaximize => false;

    /// <inheritdoc/>
    public bool SupportsMinimize => false;

    /// <inheritdoc/>
    public bool IsMaximized => false;

    /// <inheritdoc/>
    public bool IsMinimized => false;

    /// <inheritdoc/>
    /// <exception cref="PlatformNotSupportedException">Mobile windows do not expose a desktop title bar.</exception>
    public string Title
    {
        get => _game.Window.Title;
        set => throw new PlatformNotSupportedException("Mobile windows do not expose a desktop title bar.");
    }

    /// <inheritdoc/>
    /// <exception cref="PlatformNotSupportedException">Preventing platform-controlled closure is not supported.</exception>
    public bool AllowClose
    {
        get => true;
        set
        {
            if (!value)
                throw new PlatformNotSupportedException("Mobile platforms control application closure.");
        }
    }

    /// <inheritdoc/>
    /// <exception cref="PlatformNotSupportedException">Manual window resizing is not supported.</exception>
    public bool AllowResize
    {
        get => false;
        set
        {
            if (value)
                throw new PlatformNotSupportedException("Mobile windows cannot be resized manually.");
        }
    }

    /// <inheritdoc/>
    public void Maximize() => throw new PlatformNotSupportedException("Mobile windows cannot be maximized.");

    /// <inheritdoc/>
    public void Minimize() => throw new PlatformNotSupportedException("Mobile windows cannot be minimized.");

    /// <inheritdoc/>
    public void Restore() => throw new PlatformNotSupportedException("Mobile windows have no desktop window state.");
}
