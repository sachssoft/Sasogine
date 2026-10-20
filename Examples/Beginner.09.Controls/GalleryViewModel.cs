using System.ComponentModel;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Demonstrates observable application data without a property registry.</summary>
internal sealed class GalleryViewModel : Element
{
    private static readonly PropertyChangedEventArgs NotesChanged = new(nameof(Notes));
    private string _notes = "Multiline text editor\nTwo-way MVVM, selection, undo / redo";
    /// <summary>Gets or sets the notes edited through a typed two-way binding.</summary>
    public string Notes { get => _notes; set => SetNotify(ref _notes, value, NotesChanged); }
}
