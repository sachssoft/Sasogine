namespace Sachssoft.Sasogine.Android;

public class AndroidFileSystemService : IFileSystemService
{
    public string GetSpecialDirectory(SpecialDirectories directory)
    {
        switch (directory)
        {
            case SpecialDirectories.Cache:
                // Internes Cache-Verzeichnis der App
                return Application.Context.CacheDir.AbsolutePath;

            case SpecialDirectories.Temporary:
                // Android hat kein eigenes Temp, Cache ist dafür üblich
                return Application.Context.CacheDir.AbsolutePath;

            case SpecialDirectories.Application:
                // Privates App-Verzeichnis für Dateien
                return Application.Context.FilesDir.AbsolutePath;

            case SpecialDirectories.User:
                // Externer Dokumentenordner (z.B. /sdcard/Android/data/.../files)
                return Application.Context.GetExternalFilesDir(null)?.AbsolutePath
                       ?? Application.Context.FilesDir.AbsolutePath;

            default:
                throw new ArgumentOutOfRangeException(nameof(directory), directory, null);
        }
    }
}
