
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls;

/// <summary>
/// Manages an ordered collection of child controls and coordinates changes to their ownership and attachment.
/// </summary>
/// <remarks>
/// Ownership changes take effect immediately, while changes to the committed child collection
/// may be deferred until a safe traversal boundary.
/// </remarks>
public abstract class ContainerControl : Control
{
    /// <summary>
    /// Stores the child controls currently committed to this container.
    /// </summary>
    private readonly List<Control> _children = new();

    /// <summary>
    /// Stores all logically owned child controls, including pending additions.
    /// </summary>
    private readonly List<Control> _ownedChildren = new();

    /// <summary>
    /// Provides access to the committed child collection.
    /// </summary>
    private readonly ControlCollection _view;

    /// <summary>
    /// Indicates whether the cached child ordering must be rebuilt.
    /// </summary>
    private bool _orderDirty = true;

    /// <summary>
    /// Caches committed child controls in ascending Z-index order.
    /// </summary>
    private Control[] _ordered = Array.Empty<Control>();

    /// <summary>
    /// Initializes an empty container with a collection view over its committed children.
    /// </summary>
    protected ContainerControl() : base()
    {
        _view = new ControlCollection(this, _children);
    }

    /// <summary>
    /// Gets the committed child controls in insertion order.
    /// </summary>
    /// <remarks>
    /// Logical ownership reservations take effect immediately, even when changes
    /// to the committed collection are deferred.
    /// </remarks>
    public ControlCollection Children => _view;

    /// <summary>
    /// Gets the committed child controls sorted by ascending Z-index.
    /// </summary>
    /// <remarks>
    /// The cached array is rebuilt when its ordering is invalidated.
    /// Controls with equal Z-index values retain their committed insertion order.
    /// </remarks>
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

    /// <summary>
    /// Reserves ownership of a child control and schedules its addition to the committed collection.
    /// </summary>
    /// <param name="child">The unattached control to add.</param>
    public virtual void Add(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);

        uint revision = ReserveChild(child);
        Mutate(() => CommitAdd(child, revision), child.Workspace);
    }

    /// <summary>
    /// Releases ownership of a child control and schedules its removal from the committed collection.
    /// </summary>
    /// <param name="child">The control to remove from this container.</param>
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

    /// <summary>
    /// Releases all logically owned children, including additions awaiting commitment.
    /// </summary>
    public virtual void Clear()
    {
        VerifyTreeMutation();

        if (_ownedChildren.Count == 0)
            return;

        // Pending additions are included because ownership is tracked separately.
        Control[] children = _ownedChildren.ToArray();

        foreach (Control child in children)
            Remove(child);
    }

    /// <summary>
    /// Removes a local style override and restores the corresponding style value.
    /// </summary>
    /// <param name="property">The name of the style property to reset.</param>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            default:
                base.ResetStyleProperty(property);
                return;
        }
    }

    /// <summary>
    /// Replaces an owned control reference after validating the replacement.
    /// </summary>
    /// <param name="field">The field containing the currently owned control.</param>
    /// <param name="value">The replacement control, or <see langword="null"/> to clear the reference.</param>
    /// <param name="notification">The property change notification associated with the reference.</param>
    /// <param name="present">
    /// Whether the replacement control should be added to the committed child collection.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the reference changed; otherwise, <see langword="false"/>.
    /// </returns>
    protected bool SetOwnedControl(
        ref Control? field,
        Control? value,
        PropertyChangedEventArgs notification,
        bool present = true)
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

    /// <summary>
    /// Validates a control before it becomes a logical child of this container.
    /// </summary>
    /// <param name="child">The control whose ownership and ancestry must be validated.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="child"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the control already has an owner or would create an ancestry cycle.
    /// </exception>
    protected void ValidateChild(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);

        VerifyTreeMutation();
        child.Workspace?.VerifyTreeMutation();

        if (child.Parent is not null || child.RootOwner is not null)
        {
            throw new InvalidOperationException(
                "A control can have only one owner. Remove it before reparenting.");
        }

        for (Control? ancestor = this; ancestor is not null; ancestor = ancestor.Parent)
        {
            if (ReferenceEquals(ancestor, child))
                throw new InvalidOperationException("Control trees cannot contain cycles.");
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
    }

    /// <summary>
    /// Assigns a workspace to a control and recursively updates its committed descendants.
    /// </summary>
    /// <param name="control">The control to attach or detach.</param>
    /// <param name="root">The target workspace, or <see langword="null"/> to detach the control.</param>
    internal static void Attach(Control control, Workspace? root)
    {
        control.Workspace = root;
        root?.InvalidateLayout();

        if (root is null)
        {
            control.IsPointerOver = false;
            control.CancelInteraction();
        }

        if (control is ContainerControl container && ReferenceEquals(control.Workspace, root))
        {
            foreach (Control child in container.OrderedChildren)
            {
                if (ReferenceEquals(child.CommittedParent, container) &&
                    ReferenceEquals(control.Workspace, root))
                {
                    Attach(child, root);
                }
            }
        }
    }

    /// <summary>
    /// Removes a child from its committed parent using its current committed ownership revision.
    /// </summary>
    /// <param name="child">The committed child to detach.</param>
    internal void DetachCommittedChild(Control child)
    {
        CommitRemove(child, child.CommittedOwnershipRevision);
    }

    /// <summary>
    /// Marks the cached child ordering as invalid.
    /// </summary>
    internal void InvalidateOrder()
    {
        _orderDirty = true;
    }

    /// <inheritdoc/>
    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>
    /// Shows or hides an owned child in the committed collection without releasing its ownership.
    /// </summary>
    /// <param name="child">The logically owned control to update.</param>
    /// <param name="present">Whether the control should be present in the committed collection.</param>
    internal void PresentOwnedChild(Control child, bool present)
    {
        if (!ReferenceEquals(child.Parent, this))
            return;

        if (present)
            CommitAdd(child, child.OwnershipRevision);
        else
            CommitRemove(child, child.OwnershipRevision);
    }

    /// <summary>
    /// Releases logical ownership of a child control and advances its ownership revision.
    /// </summary>
    /// <param name="child">The control whose ownership should be released.</param>
    internal void ReleaseChild(Control child)
    {
        VerifyTreeMutation();

        if (!ReferenceEquals(child.Parent, this))
            return;

        RemoveReference(_ownedChildren, child);

        child.Parent = null;
        child.OwnershipRevision = unchecked(child.OwnershipRevision + 1);
    }

    /// <summary>
    /// Replaces a committed child, optionally preserving logical ownership of both controls.
    /// </summary>
    /// <param name="previous">The previously presented control, if any.</param>
    /// <param name="next">The replacement control, if any.</param>
    /// <param name="retainOwnership">
    /// Whether existing logical ownership should be retained during the replacement.
    /// </param>
    internal void ReplaceOwnedChild(
        Control? previous,
        Control? next,
        bool retainOwnership = false)
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

    /// <summary>
    /// Validates and reserves logical ownership of a child control.
    /// </summary>
    /// <param name="child">The unattached control to reserve.</param>
    /// <returns>The updated ownership revision assigned to the control.</returns>
    internal uint ReserveChild(Control child)
    {
        ValidateChild(child);

        _ownedChildren.Add(child);
        child.Parent = this;
        child.OwnershipRevision = unchecked(child.OwnershipRevision + 1);

        return child.OwnershipRevision;
    }

    /// <summary>
    /// Commits a previously reserved child if its ownership revision is still current.
    /// </summary>
    /// <param name="child">The control to add to the committed collection.</param>
    /// <param name="revision">The ownership revision associated with the reservation.</param>
    private void CommitAdd(Control child, uint revision)
    {
        if (!ReferenceEquals(child.Parent, this) || child.OwnershipRevision != revision)
            return;

        if (ReferenceEquals(child.CommittedParent, this) &&
            child.CommittedOwnershipRevision == revision)
        {
            return;
        }

        child.CommittedParent?.CommitRemove(child, child.CommittedOwnershipRevision);

        _children.Add(child);
        child.CommittedParent = this;
        child.CommittedOwnershipRevision = revision;

        _orderDirty = true;

        InvalidateMeasure();
        Attach(child, Workspace);
    }

    /// <summary>
    /// Removes a committed child if its committed ownership revision matches the requested revision.
    /// </summary>
    /// <param name="child">The control to remove from the committed collection.</param>
    /// <param name="revision">The committed ownership revision expected for the removal.</param>
    private void CommitRemove(Control child, uint revision)
    {
        if (!ReferenceEquals(child.CommittedParent, this) ||
            child.CommittedOwnershipRevision != revision)
        {
            return;
        }

        RemoveReference(_children, child);

        child.CommittedParent = null;
        _orderDirty = true;

        InvalidateMeasure();
        Attach(child, null);
    }

    /// <summary>
    /// Executes or schedules a tree mutation and invalidates the container's layout and ordering.
    /// </summary>
    /// <param name="action">The mutation to execute or schedule.</param>
    /// <param name="boundary">
    /// An optional workspace whose traversal state may determine the mutation boundary.
    /// </param>
    private void Mutate(Action action, Workspace? boundary = null)
    {
        _orderDirty = true;
        InvalidateMeasure();

        Workspace? root = Workspace;

        if (boundary?.IsTraversing == true && root?.IsTraversing != true)
            root = boundary;

        if (root is not null)
            root.Mutate(action);
        else
            action();
    }

    /// <summary>
    /// Removes the first occurrence of a control from a list using reference equality.
    /// </summary>
    /// <param name="items">The list from which to remove the control.</param>
    /// <param name="child">The control reference to locate and remove.</param>
    private static void RemoveReference(List<Control> items, Control child)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (ReferenceEquals(items[i], child))
            {
                items.RemoveAt(i);
                return;
            }
        }
    }
}
