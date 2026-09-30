using Android.Content;
using Sachssoft.Engine.Services.Platform;
using System;

namespace Sachssoft.Engine.Platform.Android;

/// <summary>Android implementation of <see cref="IClipboardService"/>.</summary>
public sealed class AndroidClipboard : IClipboardService
{
    private static ClipboardManager? Clipboard =>
        Application.Context.GetSystemService(Context.ClipboardService) as ClipboardManager;

    public void Clear() => Clipboard?.ClearPrimaryClip();

    public bool ContainsData(string format)
    {
        ArgumentNullException.ThrowIfNull(format);
        return Clipboard?.HasPrimaryClip == true;
    }

    public object? GetData(string format)
    {
        ArgumentNullException.ThrowIfNull(format);
        var clip = Clipboard?.PrimaryClip;
        if (clip is null || clip.ItemCount == 0)
            return null;
        return clip.GetItemAt(0)?.CoerceToText(Application.Context)?.ToString();
    }

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

        Clipboard?.SetPrimaryClip(ClipData.NewPlainText(format, text));
    }
}
