using Foundation;
using Sachssoft.Engine.Services.Platform;
using System;
using System.IO;
using UIKit;

namespace Sachssoft.Engine.Platform.iOS;

/// <summary>iOS implementation of <see cref="ILauncherService"/>.</summary>
public sealed class iOSLauncher : ILauncherService
{
    public bool TryReveal(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path) || (!File.Exists(path) && !Directory.Exists(path)))
                return false;

            // iOS does not provide a general Explorer-style "reveal in folder" operation.
            return false;
        }
        catch
        {
            return false;
        }
    }

    public bool TryOpenUri(Uri uri)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(uri);
            if (!uri.IsAbsoluteUri)
                return false;

            using var nativeUri = NSUrl.FromString(uri.AbsoluteUri);
            if (nativeUri is null || !UIApplication.SharedApplication.CanOpenUrl(nativeUri))
                return false;

            UIApplication.SharedApplication.OpenUrl(nativeUri, new UIApplicationOpenUrlOptions(), null);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
