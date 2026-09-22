
namespace Sachssoft.Engine;

/// <summary>
/// Specifies how definition types are matched against registered definition types.
/// </summary>
public enum DefinitionMatchMode
{
    /// <summary>
    /// Requires the definition type to exactly match the registered type.
    /// </summary>
    Exact,

    /// <summary>
    /// Allows derived definition types to match a registered base type.
    /// </summary>
    Assignable
}