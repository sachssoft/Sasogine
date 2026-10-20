namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Explicit application data for the typed column delegates.</summary>
internal sealed class ControlsDemoRow
{
    internal ControlsDemoRow(string name, string version, int downloads, string license)
    {
        Name = name;
        Version = version;
        Downloads = downloads;
        License = license;
    }

    internal string Name { get; set; }
    internal string Version { get; set; }
    internal int Downloads { get; set; }
    internal string License { get; set; }
}
