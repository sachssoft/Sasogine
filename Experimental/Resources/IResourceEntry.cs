namespace Sachssoft.Sasogine.FeatureLabs.Resources;

public interface IResourceEntry
{
    /// <summary>
    /// Eindeutige Id des Skin-Eintrags.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Optional zugeordnete Klasse für Gruppierungen.
    /// </summary>
    string? Class { get; }
}
