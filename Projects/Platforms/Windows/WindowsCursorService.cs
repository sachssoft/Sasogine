using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Services.Platform;
using System;

namespace Sachssoft.Engine.Platform.Windows;

/// <summary>
/// Implements native Windows cursors through the active MonoGame desktop backend.
/// </summary>
/// <remarks>
/// Use one instance per game and call it on the game thread after window initialization.
/// Dispose the service before disposing the game. Standard cursors are shared with MonoGame;
/// only custom cursors created by this service are owned and disposed by it.
/// </remarks>
public sealed class WindowsCursorService : IPlatformCursorService, IDisposable
{
    /// <summary>Stores the game whose native cursor visibility is controlled.</summary>
    private readonly Game _game;

    /// <summary>Stores the owning game thread identifier.</summary>
    private readonly int _threadId;

    /// <summary>Stores whether the service has released its resources.</summary>
    private bool _disposed;

    /// <summary>Stores the currently owned custom cursor, if any.</summary>
    private MouseCursor? _customCursor;

    /// <summary>
    /// Initializes the native cursor service for a game on its owning thread.
    /// </summary>
    /// <param name="game">The game whose native cursor is controlled.</param>
    /// <exception cref="ArgumentNullException">The game is null.</exception>
    public WindowsCursorService(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        _game = game;
        _threadId = Environment.CurrentManagedThreadId;
    }

    /// <inheritdoc/>
    public bool IsVisible
    {
        get
        {
            VerifyAccess();
            return _game.IsMouseVisible;
        }
        set
        {
            VerifyAccess();
            if (_game.IsMouseVisible != value)
                _game.IsMouseVisible = value;
        }
    }

    /// <summary>Selects a standard Windows cursor without changing its visibility.</summary>
    /// <param name="cursorId">The cursor to select; unknown values use the standard arrow.</param>
    public void SetCursor(WindowsCursorIds cursorId)
    {
        VerifyAccess();

        MouseCursor cursor = cursorId switch
        {
            WindowsCursorIds.Hand => MouseCursor.Hand,
            WindowsCursorIds.Text => MouseCursor.IBeam,
            WindowsCursorIds.Crosshair => MouseCursor.Crosshair,
            WindowsCursorIds.Move => MouseCursor.SizeAll,
            WindowsCursorIds.ResizeHorizontal => MouseCursor.SizeWE,
            WindowsCursorIds.ResizeVertical => MouseCursor.SizeNS,
            WindowsCursorIds.ResizeDiagonalDown => MouseCursor.SizeNWSE,
            WindowsCursorIds.ResizeDiagonalUp => MouseCursor.SizeNESW,
            WindowsCursorIds.NotAllowed => MouseCursor.No,
            WindowsCursorIds.Wait => MouseCursor.Wait,
            WindowsCursorIds.WaitArrow => MouseCursor.WaitArrow,
            _ => MouseCursor.Arrow
        };

        Mouse.SetCursor(cursor);
        ReleaseCustomCursor();
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">The texture is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The hotspot is outside the texture.</exception>
    /// <exception cref="ArgumentException">The texture does not use the Color surface format.</exception>
    /// <exception cref="ObjectDisposedException">The texture has been disposed.</exception>
    public void SetCustomCursor(Texture2D texture, PixelPoint2 hotspot)
    {
        VerifyAccess();
        ArgumentNullException.ThrowIfNull(texture);
        ObjectDisposedException.ThrowIf(texture.IsDisposed, texture);

        if (hotspot.X < 0 || hotspot.Y < 0 || hotspot.X >= texture.Width || hotspot.Y >= texture.Height)
            throw new ArgumentOutOfRangeException(nameof(hotspot), "The hotspot must be inside the texture.");

        if (texture.Format != SurfaceFormat.Color)
            throw new ArgumentException("Cursor textures must use the Color surface format.", nameof(texture));

        MouseCursor cursor = MouseCursor.FromTexture2D(texture, hotspot.X, hotspot.Y);
        try
        {
            Mouse.SetCursor(cursor);
        }
        catch
        {
            cursor.Dispose();
            throw;
        }

        ReleaseCustomCursor();
        _customCursor = cursor;
    }

    /// <inheritdoc/>
    public void ResetCursor()
    {
        SetCursor(WindowsCursorIds.Arrow);
    }

    /// <summary>
    /// Restores the arrow before releasing an owned custom cursor. Repeated disposal has no effect.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        VerifyAccess();
        if (_customCursor is not null)
        {
            Mouse.SetCursor(MouseCursor.Arrow);
            ReleaseCustomCursor();
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    void IPlatformCursorService.SetCursor(int cursorId)
    {
        SetCursor((WindowsCursorIds)cursorId);
    }

    /// <summary>Releases only the custom cursor owned by this service.</summary>
    private void ReleaseCustomCursor()
    {
        _customCursor?.Dispose();
        _customCursor = null;
    }

    /// <summary>Checks lifetime and access from the owning game thread.</summary>
    /// <exception cref="ObjectDisposedException">The service has been disposed.</exception>
    /// <exception cref="InvalidOperationException">The call originates from a different thread.</exception>
    private void VerifyAccess()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (Environment.CurrentManagedThreadId != _threadId)
            throw new InvalidOperationException("Cursor services must be used on their owning game thread.");
    }
}
