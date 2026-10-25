namespace Sachssoft.Engine.Controls;
/// <summary>Separates groups of menu items without exposing command or checked-state behavior.</summary>
/// <remarks>
/// Place instances in a MenuBar, ContextMenu or MenuItem.Items collection.
/// The containing menu draws the separator with its existing skin or solid border color.
/// Separators cannot be activated, selected by keyboard navigation or matched by shortcuts.
/// </remarks>
public sealed class MenuSeparator : MenuEntry
{
}
