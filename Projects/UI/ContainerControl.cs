using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Engine.Controls;
/// <summary>Owns ordered child controls. Traversal-time changes commit at a frame boundary.</summary>
public abstract class ContainerControl : Control
{
    private readonly List<Control> _children = new();
    private readonly List<Control> _ownedChildren = new();
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
    /// <summary>Reserves an unattached control immediately. Frame traversal changes commit at a safe boundary.</summary>
    public virtual void Add(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        uint revision = ReserveChild(child);
        Mutate(() => CommitAdd(child, revision), child.Workspace);
    }

    /// <summary>Releases ownership immediately and removes the committed child at a safe boundary.</summary>
    public virtual void Remove(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        VerifyTreeMutation();
        if (!ReferenceEquals(child.Parent, this))
            return;
        uint revision = child.OwnershipRevision;
        ReleaseChild(child);
        Mutate(() => CommitRemove(child, revision));
    }

    /// <summary>Releases all children, including additions pending the next boundary.</summary>
    public virtual void Clear()
    {
        VerifyTreeMutation();
        if (_ownedChildren.Count == 0)
            return;
        // Use the ownership list rather than the committed list: pending additions also belong here.
        Control[] children = _ownedChildren.ToArray();
        foreach (Control child in children)
            Remove(child);
    }

    /// <summary>Atomically replaces an owned property, validating before changing its previous value.</summary>
    protected bool SetOwnedControl(ref Control? field, Control? value,
        System.ComponentModel.PropertyChangedEventArgs notification, bool present = true)
    {
        if (ReferenceEquals(field, value))
            return false;
        VerifyTreeMutation();
        uint nextRevision = value is null ? 0 : ReserveChild(value);
        Control? previous = field;
        uint previousRevision = previous?.OwnershipRevision ?? 0;
        if (previous is not null)
            ReleaseChild(previous);
        field = value;
        Workspace? boundary = value?.Workspace ?? previous?.Workspace;
        Mutate(() =>
        {
            if (previous is not null)
                CommitRemove(previous, previousRevision);
            if (value is not null && present)
                CommitAdd(value, nextRevision);
        }, boundary);
        NotifyPropertyChanged(notification);
        return true;
    }

    internal uint ReserveChild(Control child)
    {
        ValidateChild(child);
        _ownedChildren.Add(child);
        child.Parent = this;
        child.OwnershipRevision = unchecked(child.OwnershipRevision + 1);
        return child.OwnershipRevision;
    }

    internal void ReleaseChild(Control child)
    {
        VerifyTreeMutation();
        if (!ReferenceEquals(child.Parent, this))
            return;
        _ownedChildren.Remove(child);
        child.Parent = null;
        child.OwnershipRevision = unchecked(child.OwnershipRevision + 1);
    }

    // Internal presenters may hide a control while retaining its logical ownership.
    internal void PresentOwnedChild(Control child, bool present)
    {
        if (!ReferenceEquals(child.Parent, this))
            return;
        if (present)
            CommitAdd(child, child.OwnershipRevision);
        else
            CommitRemove(child, child.OwnershipRevision);
    }

    internal void ReplaceOwnedChild(Control? previous, Control? next, bool retainOwnership = false)
    {
        if (ReferenceEquals(previous, next))
        {
            if (next is not null)
                PresentOwnedChild(next, true);
            return;
        }
        uint nextRevision = next?.OwnershipRevision ?? 0;
        if (next is not null && !retainOwnership)
            nextRevision = ReserveChild(next);
        if (previous is not null)
        {
            uint previousRevision = previous.CommittedOwnershipRevision;
            if (!retainOwnership)
                ReleaseChild(previous);
            CommitRemove(previous, previousRevision);
        }
        if (next is not null)
            CommitAdd(next, nextRevision);
    }

    private void CommitAdd(Control child, uint revision)
    {
        if (!ReferenceEquals(child.Parent, this) || child.OwnershipRevision != revision)
            return; // A later assignment superseded this queued addition.
        if (ReferenceEquals(child.CommittedParent, this) && child.CommittedOwnershipRevision == revision)
            return;
        child.CommittedParent?.CommitRemove(child, child.CommittedOwnershipRevision);
        _children.Add(child);
        child.CommittedParent = this;
        child.CommittedOwnershipRevision = revision;
        _orderDirty = true;
        InvalidateMeasure();
        Attach(child, Workspace);
    }

    private void CommitRemove(Control child, uint revision)
    {
        if (!ReferenceEquals(child.CommittedParent, this) || child.CommittedOwnershipRevision != revision)
            return; // Never detach a control committed by a newer owner or assignment.
        _children.Remove(child);
        child.CommittedParent = null;
        _orderDirty = true;
        InvalidateMeasure();
        Attach(child, null);
    }

    internal void DetachCommittedChild(Control child) => CommitRemove(child, child.CommittedOwnershipRevision);

    private void Mutate(Action action, Workspace? boundary = null)
    {
        _orderDirty = true;
        InvalidateMeasure();
        Workspace? root = Workspace;
        if (boundary?.IsTraversing == true && root?.IsTraversing != true)
            root = boundary;
        if (root is not null) root.Mutate(action);
        else action();
    }

    internal void VerifyTreeMutation()
    {
        Workspace?.VerifyTreeMutation();
    }

    protected void ValidateChild(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        VerifyTreeMutation();
        child.Workspace?.VerifyTreeMutation();
        if (child.Parent is not null || child.RootOwner is not null)
            throw new InvalidOperationException("A control can have only one owner. Remove it before reparenting.");
        for (Control? ancestor = this; ancestor is not null; ancestor = ancestor.Parent)
            if (ReferenceEquals(ancestor, child))
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
