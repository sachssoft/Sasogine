namespace Sachssoft.Sasogine.FeatureLabs.Resources;

[Flags]
public enum ResourceLookupOptions
{
    None = 0,
    IncludeHierarchy = 1 << 0,  // Suche auch in übergeordneten Containern
    Reload = 1 << 1,
}
