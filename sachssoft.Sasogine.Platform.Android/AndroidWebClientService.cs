namespace sachssoft.Sasogine.Platform.Android;

public class AndroidWebClientService : IWebClientService
{
    public void Open(Uri uri)
    {
        var intent = new Intent(Intent.ActionView, Uri.Parse(uri.AbsoluteUri));
        intent.AddFlags(ActivityFlags.NewTask);

        // Context von Application abrufen
        Application.Context.StartActivity(intent);
    }
}
