using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components.Tools.Selection;

/// <summary>
/// Represents an interaction node used by a selection tool layer.
/// </summary>
public sealed class SelectionToolNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectionToolNode"/> class
    /// with the specified shape.
    /// </summary>
    /// <param name="shape">
    /// The shape used to represent the node.
    /// </param>
    /// <param name="isVisible">
    /// Indicates whether the node is visible.
    /// </param>
    /// <param name="opacity">
    /// The opacity of the node.
    /// </param>
    public SelectionToolNode(
        SelectionToolNodeShape shape,
        bool isVisible = true,
        float opacity = 1f)
    {
        Shape = shape;
        IsVisible = isVisible;
        Opacity = opacity;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectionToolNode"/> class
    /// with the specified shape, position, and size.
    /// </summary>
    /// <param name="shape">
    /// The shape used to represent the node.
    /// </param>
    /// <param name="position">
    /// The position of the node relative to its selection target.
    /// </param>
    /// <param name="size">
    /// The size of the node.
    /// </param>
    /// <param name="isVisible">
    /// Indicates whether the node is visible.
    /// </param>
    /// <param name="opacity">
    /// The opacity of the node.
    /// </param>
    public SelectionToolNode(
        SelectionToolNodeShape shape,
        Vector2 position,
        Size2 size,
        bool isVisible = true,
        float opacity = 1f)
        : this(shape, isVisible, opacity)
    {
        Position = position;
        Size = size;
    }

    /// <summary>
    /// Gets the shape used to represent the node.
    /// </summary>
    public SelectionToolNodeShape Shape { get; }

    /// <summary>
    /// Gets or sets the position of the node relative to its selection target.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Gets or sets the size of the node.
    /// </summary>
    public Size2 Size { get; set; }

    /// <summary>
    /// Gets a value indicating whether the node is visible.
    /// </summary>
    public bool IsVisible { get; } = true;

    /// <summary>
    /// Gets the opacity of the node.
    /// </summary>
    public float Opacity { get; } = 1f;
}