using Android.Content;
using Sachssoft.Engine.Services.Platform;
using System;
using System.IO;

namespace Sachssoft.Engine.Platform.Android;

/// <summary>Android implementation of <see cref="ILauncherService"/>.</summary>
public sealed class AndroidLauncher : ILauncherService
{
    public bool TryReveal(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path) || (!File.Exists(path) && !Directory.Exists(path)))
                return false;

            // Android has no universal Explorer-style "reveal in folder" operation.
            return false;
        }
        catch
        {
            return false;
        }
    }

    public bool TryOpenUri(System.Uri uri)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(uri);
            if (!uri.IsAbsoluteUri)
                return false;

            var androidUri = global::Android.Net.Uri.Parse(uri.AbsoluteUri);
            if (androidUri is null)
                return false;

            var intent = new Intent(Intent.ActionView, androidUri);
            intent.AddFlags(ActivityFlags.NewTask);
            Application.Context.StartActivity(intent);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
