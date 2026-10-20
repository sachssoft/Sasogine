using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Small document with selection, immutable object values and bounded undo history.</summary>
internal sealed class EditorDocument
{
    private readonly List<EditorObject> _objects = new();
    private readonly List<EditorSnapshot> _undo = new(), _redo = new();
    private readonly ReadOnlyCollection<EditorObject> _view;
    private EditorObject[] _saved = [];
    private EditorSnapshot? _pending;
    internal EditorDocument()
    {
        _view = _objects.AsReadOnly();
        New();
    }

    internal IReadOnlyList<EditorObject> Objects => _view;
    internal int? Selection { get; private set; }
    internal EditorObject? Selected => _objects.Find(o => o.Id == Selection);
    internal bool CanUndo => _pending is null && _undo.Count > 0;
    internal bool CanRedo => _pending is null && _redo.Count > 0;
    internal bool IsEditing => _pending is not null;
    internal bool IsDirty => !_objects.SequenceEqual(_saved);

    internal event Action? Changed;
    internal void Select(int? id)
    {
        Selection = _objects.Any(o => o.Id == id) ? id : null;
        Changed?.Invoke();
    }

    private EditorSnapshot Snapshot() => new(_objects.ToArray(), Selection);
    private void Restore(EditorSnapshot state)
    {
        _objects.Clear();
        _objects.AddRange(state.Objects);
        Selection = state.Selection;
        Changed?.Invoke();
    }

    private static void Push(List<EditorSnapshot> stack, EditorSnapshot state)
    {
        stack.Add(state);
        if (stack.Count > 64)
            stack.RemoveAt(0);
    }

    private void Commit(Action edit)
    {
        if (IsEditing)
            return;
        var before = Snapshot();
        edit();
        if (!before.Objects.SequenceEqual(_objects))
        {
            Push(_undo, before);
            _redo.Clear();
        }

        Changed?.Invoke();
    }

    internal void New() => Commit(() =>
    {
        _objects.Clear();
        _objects.Add(new(1, "Welcome card", 100, 80, 360, 160, new Color(40, 148, 176)));
        _objects.Add(new(2, "Action panel", 520, 220, 280, 220, new Color(124, 94, 190)));
        _objects.Add(new(3, "Accent", 140, 320, 240, 100, new Color(210, 143, 62)));
        Selection = 1;
    });
    internal void Add(bool duplicate = false) => Commit(() =>
    {
        if (_objects.Count >= 256)
            return;
        int id = 1;
        while (_objects.Any(o => o.Id == id))
            id++;
        var source = duplicate ? Selected : null;
        var item = source is null ? new EditorObject(id, $"Rectangle {id}", 200, 160, 220, 120, new Color(65, 170, 190)) : source with
        {
            Id = id,
            Name = source.Name[..Math.Min(source.Name.Length, 75)] + " copy",
            X = Math.Min(960 - source.Width, source.X + 24),
            Y = Math.Min(540 - source.Height, source.Y + 24)
        };
        _objects.Add(item);
        Selection = id;
    });
    internal void Delete() => Commit(() =>
    {
        _objects.RemoveAll(o => o.Id == Selection);
        Selection = _objects.LastOrDefault()?.Id;
    });
    internal void Update(EditorObject item) => Commit(() => Replace(item));
    private void Replace(EditorObject item)
    {
        EditorFile.Validate(item);
        int index = _objects.FindIndex(o => o.Id == item.Id);
        if (index >= 0)
            _objects[index] = item;
    }

    internal void BeginDrag()
    {
        if (_pending is null && Selected is not null)
            _pending = Snapshot();
    }

    internal void Drag(float x, float y)
    {
        if (_pending is null || Selected is not EditorObject item)
            return;
        Replace(item with { X = Math.Clamp(x, 0, 960 - item.Width), Y = Math.Clamp(y, 0, 540 - item.Height) });
        Changed?.Invoke();
    }

    internal void Rotate(float angle)
    {
        if (Selected is not EditorObject item)
            return;
        item = item with
        {
            RotationDegrees = RotationControl.NormalizeAngle(angle)
        };
        if (_pending is null)
            Update(item);
        else
        {
            Replace(item);
            Changed?.Invoke();
        }
    }

    internal void EndDrag(bool cancel)
    {
        if (_pending is not EditorSnapshot before)
            return;
        _pending = null;
        if (cancel)
            Restore(before);
        else
        {
            if (!before.Objects.SequenceEqual(_objects))
            {
                Push(_undo, before);
                _redo.Clear();
            }

            Changed?.Invoke();
        }
    }

    internal void Undo()
    {
        if (!CanUndo)
            return;
        Push(_redo, Snapshot());
        var state = _undo[^1];
        _undo.RemoveAt(_undo.Count - 1);
        Restore(state);
    }

    internal void Redo()
    {
        if (!CanRedo)
            return;
        Push(_undo, Snapshot());
        var state = _redo[^1];
        _redo.RemoveAt(_redo.Count - 1);
        Restore(state);
    }

    internal void Import(IReadOnlyList<EditorObject> objects)
    {
        // Validate before touching the current document or history.
        EditorFile.ValidateAll(objects);
        Commit(() =>
        {
            _objects.Clear();
            _objects.AddRange(objects);
            Selection = _objects.FirstOrDefault()?.Id;
        });
        MarkSaved();
    }

    internal void MarkSaved()
    {
        _saved = _objects.ToArray();
        Changed?.Invoke();
    }
}
