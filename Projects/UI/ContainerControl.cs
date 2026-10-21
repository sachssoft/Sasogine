using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Engine.Controls;
/// <summary>Owns ordered child controls. Traversal-time changes commit at a frame boundary.</summary>
public abstract class ContainerControl : Control
{
    private readonly List<Control> _children = new();
    private readonly ControlCollection _view;
    /// <summary>Initializes an empty container.</summary>
    protected ContainerControl() : base()
    {
        _view = new ControlCollection(this, _children);
    }

    /// <summary>Gets children in insertion order.</summary>
    public ControlCollection Children => _view;

    private Control[] _ordered = Array.Empty<Control>();
    private bool _orderDirty = true;
    internal Control[] OrderedChildren
    {
        get
        {
            if (!_orderDirty)
                return _ordered;
            if (_ordered.Length != _children.Count)
                _ordered = new Control[_children.Count];
            _children.CopyTo(_ordered);
            for (int i = 1; i < _ordered.Length; i++)
            {
                var item = _ordered[i];
                int j = i - 1;
                while (j >= 0 && _ordered[j].ZIndex > item.ZIndex)
                {
                    _ordered[j + 1] = _ordered[j];
                    j--;
                }

                _ordered[j + 1] = item;
            }

            _orderDirty = false;
            return _ordered;
        }
    }

    internal void InvalidateOrder() => _orderDirty = true;
    /// <summary>Adds an unattached control. Mutations during traversal are deferred.</summary>
    public virtual void Add(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        ValidateChild(child);
        Mutate(() =>
        {
            ValidateChild(child);
            _children.Add(child);
            child.Parent = this;
            Attach(child, Workspace);
        });
    }

    /// <summary>Removes a child if present; during traversal this is deferred.</summary>
    public virtual void Remove(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        Mutate(() =>
        {
            if (!_children.Remove(child))
                return;
            child.Parent = null;
            Attach(child, null);
        });
    }

    /// <summary>Removes all children; during traversal this is deferred.</summary>
    public virtual void Clear() => Mutate(() =>
    {
        foreach (Control child in _children)
        {
            child.Parent = null;
            Attach(child, null);
        }

        _children.Clear();
    });
    // Called only during the root's preparation phase, before enumerating children.
    internal void ReplaceOwnedChild(Control? previous, Control? next)
    {
        if (ReferenceEquals(previous, next))
            return;
        if (next is not null)
            ValidateChild(next);
        // Internal replacements bypass Mutate, so refresh both traversal and layout caches.
        _orderDirty = true;
        InvalidateMeasure();
        if (previous is not null)
        {
            _children.Remove(previous);
            previous.Parent = null;
            Attach(previous, null);
        }

        if (next is not null)
        {
            _children.Add(next);
            next.Parent = this;
            Attach(next, Workspace);
        }
    }

    private void Mutate(Action action)
    {
        _orderDirty = true;
        InvalidateMeasure();
        if (Workspace is Workspace root)
            root.Mutate(action);
        else
            action();
    }

    private void ValidateChild(Control child)
    {
        if (child.Parent is not null || child.Workspace is not null)
            throw new InvalidOperationException("A control can have only one owner. Remove it before reparenting.");
        for (Control? ancestor = this; ancestor is not null; ancestor = ancestor.Parent)
            if (ancestor == child)
                throw new InvalidOperationException("Control trees cannot contain cycles.");
    }

    internal static void Attach(Control control, Workspace? root)
    {
        control.Workspace = root;
        root?.InvalidateLayout();
        if (root is null)
        {
            control.IsPointerOver = false;
            control.CancelInteraction();
        }

        if (control is ContainerControl container)
            foreach (Control child in container._children)
                Attach(child, root);
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            default:
                base.ResetStyleProperty(property);
                return;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
    }
}
