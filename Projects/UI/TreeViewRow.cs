namespace Sachssoft.Engine.Controls;
/// <summary>One visible row without a per-row Control instance.</summary>
internal readonly record struct TreeViewRow(TreeViewItem Item, TreeViewItem? Parent, int Depth, bool IsEnabled);
