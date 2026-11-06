using Android.App;
using Android.Content;
using Sachssoft.Engine.Services.Platform;
using System;

namespace Sachssoft.Engine.Platform.Android;

/// <summary>Android implementation of <see cref="IClipboardService"/>.</summary>
public sealed class AndroidClipboard : IClipboardService
{
    /// <summary>Gets the clipboard service exposed by the Android application context.</summary>
    private static ClipboardManager? Clipboard =>
        Application.Context.GetSystemService(Context.ClipboardService) as ClipboardManager;

    /// <inheritdoc/>
    /// <remarks>Before Android 9, clearing replaces the primary clip with empty text.</remarks>
    public void Clear()
    {
        var clipboard = Clipboard;
        if (clipboard is null)
            return;

        if (OperatingSystem.IsAndroidVersionAtLeast(28))
            clipboard.ClearPrimaryClip();
        else
            clipboard.PrimaryClip = ClipData.NewPlainText(string.Empty, string.Empty);
    }

    /// <inheritdoc/>
    public bool ContainsData(string format)
    {
        ArgumentNullException.ThrowIfNull(format);
        return Clipboard?.HasPrimaryClip == true;
    }

    /// <inheritdoc/>
    public object? GetData(string format)
    {
        ArgumentNullException.ThrowIfNull(format);
        var clip = Clipboard?.PrimaryClip;
        if (clip is null || clip.ItemCount == 0)
            return null;
        return clip.GetItemAt(0)?.CoerceToText(Application.Context)?.ToString();
    }

    /// <inheritdoc/>
    public void SetData(string format, object? data)
    {
        ArgumentNullException.ThrowIfNull(format);
        if (data is null)
        {
            Clear();
            return;
        }

        var text = data switch
        {
            string value => value,
            byte[] bytes => Convert.ToBase64String(bytes),
            _ => throw new NotSupportedException("The Android clipboard implementation currently supports string and byte[] data only.")
        };

        var clipboard = Clipboard;
        if (clipboard is not null)
            clipboard.PrimaryClip = ClipData.NewPlainText(format, text);
    }
}
