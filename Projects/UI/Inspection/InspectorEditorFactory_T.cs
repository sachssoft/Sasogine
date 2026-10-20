using System.ComponentModel;

namespace Sachssoft.Engine.Controls.Inspection;
/// <summary>Creates and adds an editor for an explicitly bound value.</summary>
/// <typeparam name = "T">The source value type.</typeparam>
/// <param name = "view">The receiving inspector.</param>
/// <param name = "title">The property label.</param>
/// <param name = "read">The source getter.</param>
/// <param name = "write">The source setter.</param>
/// <param name = "source">An optional notification source.</param>
/// <param name = "propertyName">The source property name.</param>
/// <returns>The added control.</returns>
public delegate Control InspectorEditorFactory<T>(InspectorView view, string title, Func<T> read, Action<T> write, INotifyPropertyChanged? source, string? propertyName);
