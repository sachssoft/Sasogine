namespace sachssoft.Sasogine.Platform.Android;

public class AndroidFileOpenerService : ILocalFileOpenerService
{
    public void Open(string path)
    {
        var context = Application.Context;

        var file = new Java.IO.File(path);
        if (!file.Exists())
            throw new ArgumentException($"File not found: {path}");

        // MimeType ermitteln (einfache Variante)
        string mimeType = MimeTypeMap.GetSingleton().GetMimeTypeFromExtension(
            MimeTypeMap.GetFileExtensionFromUrl(path).ToLower()) ?? "*/*";

        var fileUri = FileProvider.GetUriForFile(context, context.PackageName + ".fileprovider", file);

        var intent = new Intent(Intent.ActionView);
        intent.SetDataAndType(fileUri, mimeType);
        intent.SetFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.NewTask);

        context.StartActivity(intent);
    }
}
