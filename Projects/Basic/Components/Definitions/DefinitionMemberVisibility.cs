namespace Sachssoft.Engine.Components.Definitions;

/// <summary>
/// Specifies the visibility of a definition member.
/// </summary>
public enum DefinitionMemberVisibility
{
    /// <summary>
    /// The member is normally visible.
    /// </summary>
    Visible,

    /// <summary>
    /// The member is only visible when advanced members are shown.
    /// </summary>
    Advanced,

    /// <summary>
    /// The member is never displayed.
    /// </summary>
    Never
}