namespace Sachssoft.Sasogine.Containers
{
    public class PublishOptions
    {
        public bool Encrypt { get; set; } = false;         // Verschlüsselung aktivieren
        public bool Compress { get; set; } = false;        // ZIP-Kompression aktivieren
        public string? Password { get; set; } = null;      // Passwort für Verschlüsselung
        public string OutputFilePath { get; set; } = "published_package.spk";
    }
}
