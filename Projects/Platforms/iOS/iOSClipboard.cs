using Sachssoft.Engine.Services.Platform;
using System;
using UIKit;

namespace Sachssoft.Engine.Platform.iOS;

/// <summary>iOS implementation of <see cref="IClipboardService"/>.</summary>
public sealed class iOSClipboard : IClipboardService
{
    public void Clear() => UIPasteboard.General.String = null;

    public bool ContainsData(string format)
    {
        ArgumentNullException.ThrowIfNull(format);
        return UIPasteboard.General.HasStrings;
    }

    public object? GetData(string format)
    {
        ArgumentNullException.ThrowIfNull(format);
        return UIPasteboard.General.String;
    }

    public void SetData(string format, object? data)
    {
        ArgumentNullException.ThrowIfNull(format);
        if (data is null)
        {
            Clear();
            return;
        }

        UIPasteboard.General.String = data switch
        {
            string value => value,
            byte[] bytes => Convert.ToBase64String(bytes),
            _ => throw new NotSupportedException("The iOS clipboard implementation currently supports string and byte[] data only.")
        };
    }
}
