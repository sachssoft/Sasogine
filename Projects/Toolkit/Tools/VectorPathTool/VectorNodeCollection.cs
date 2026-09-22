using System;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Represents a mutable collection of control nodes owned by a vector segment.
/// </summary>
public sealed class VectorNodeCollection :
    IList<VectorNode>,
    IReadOnlyList<VectorNode>
{
    private readonly List<VectorNode> _nodes = [];
    private readonly IVectorSegment _segment;

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorNodeCollection"/> class.
    /// </summary>
    /// <param name="segment">
    /// The vector segment that owns the collection.
    /// </param>
    internal VectorNodeCollection(IVectorSegment segment)
    {
        _segment = segment
            ?? throw new ArgumentNullException(nameof(segment));
    }

    /// <summary>
    /// Gets or sets the node at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index of the node to get or set.
    /// </param>
    /// <returns>
    /// The node at the specified index.
    /// </returns>
    public VectorNode this[int index]
    {
        get => _nodes[index];
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            VectorNode current = _nodes[index];

            if (ReferenceEquals(current, value))
                return;

            EnsureCanAttach(value);

            current.Segment = null;
            value.Segment = _segment;
            _nodes[index] = value;

            SynchronizeDefinition();
        }
    }

    /// <summary>
    /// Gets the number of nodes contained in the collection.
    /// </summary>
    public int Count => _nodes.Count;

    /// <summary>
    /// Gets a value indicating whether the collection is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Adds a node to the collection.
    /// </summary>
    /// <param name="item">
    /// The node to add.
    /// </param>
    public void Add(VectorNode item)
    {
        ArgumentNullException.ThrowIfNull(item);

        EnsureCanAttach(item);

        item.Segment = _segment;
        _nodes.Add(item);

        SynchronizeDefinition();
    }

    /// <summary>
    /// Adds the specified nodes to the collection.
    /// </summary>
    /// <param name="items">
    /// The nodes to add.
    /// </param>
    public void AddRange(IEnumerable<VectorNode> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var range = new List<VectorNode>(items);

        for (int i = 0; i < range.Count; i++)
        {
            ArgumentNullException.ThrowIfNull(range[i]);
            EnsureCanAttach(range[i]);
        }

        for (int i = 0; i < range.Count; i++)
            Add(range[i]);
    }

    /// <summary>
    /// Removes all nodes from the collection.
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < _nodes.Count; i++)
            _nodes[i].Segment = null;

        _nodes.Clear();

        SynchronizeDefinition();
    }

    /// <summary>
    /// Determines whether the collection contains the specified node.
    /// </summary>
    /// <param name="item">
    /// The node to locate.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the node is contained in the collection;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(VectorNode item) =>
        _nodes.Contains(item);

    /// <summary>
    /// Copies the nodes to the specified array, starting at the specified index.
    /// </summary>
    /// <param name="array">
    /// The destination array.
    /// </param>
    /// <param name="arrayIndex">
    /// The zero-based index in the destination array at which copying begins.
    /// </param>
    public void CopyTo(
        VectorNode[] array,
        int arrayIndex) =>
        _nodes.CopyTo(array, arrayIndex);

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// An enumerator for the collection.
    /// </returns>
    public IEnumerator<VectorNode> GetEnumerator() =>
        _nodes.GetEnumerator();

    /// <summary>
    /// Determines the index of the specified node.
    /// </summary>
    /// <param name="item">
    /// The node to locate.
    /// </param>
    /// <returns>
    /// The zero-based index of the node if found; otherwise, <c>-1</c>.
    /// </returns>
    public int IndexOf(VectorNode item) =>
        _nodes.IndexOf(item);

    /// <summary>
    /// Inserts a node at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which the node is inserted.
    /// </param>
    /// <param name="item">
    /// The node to insert.
    /// </param>
    public void Insert(
        int index,
        VectorNode item)
    {
        ArgumentNullException.ThrowIfNull(item);

        EnsureCanAttach(item);

        item.Segment = _segment;
        _nodes.Insert(index, item);

        SynchronizeDefinition();
    }

    /// <summary>
    /// Removes the specified node from the collection.
    /// </summary>
    /// <param name="item">
    /// The node to remove.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the node was removed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Remove(VectorNode item)
    {
        if (!_nodes.Remove(item))
            return false;

        item.Segment = null;

        SynchronizeDefinition();

        return true;
    }

    /// <summary>
    /// Removes the node at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index of the node to remove.
    /// </param>
    public void RemoveAt(int index)
    {
        VectorNode item = _nodes[index];

        _nodes.RemoveAt(index);
        item.Segment = null;

        SynchronizeDefinition();
    }

    /// <summary>
    /// Reverses the order of the nodes in the collection.
    /// </summary>
    public void Reverse()
    {
        _nodes.Reverse();

        SynchronizeDefinition();
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    private void SynchronizeDefinition()
    {
        if (_segment.Definition is not VectorVariableSegmentDefinition definition)
            return;

        definition.ControlNodes.Clear();

        for (int i = 0; i < _nodes.Count; i++)
            definition.ControlNodes.Add(_nodes[i].Definition);
    }

    private static void EnsureCanAttach(VectorNode node)
    {
        if (node.Segment != null)
        {
            throw new InvalidOperationException(
                "The vector node already belongs to a vector segment.");
        }
    }
}