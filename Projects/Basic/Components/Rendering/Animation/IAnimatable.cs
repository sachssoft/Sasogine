using Sachssoft.Engine;
using System.Collections.ObjectModel;

namespace Sachssoft.Engine.Components.Rendering;

/// <summary>
/// Represents an object that supports animations.
/// </summary>
public interface IAnimatable
{
    /// <summary>
    /// Gets the collection of animations applied to this object.
    /// </summary>
    ObservableCollection<IAnimationComponent> Animations { get; }

    /// <summary>
    /// Gets the initial position of the object when animations start.
    /// </summary>
    Point2 StartPosition { get; }

    /// <summary>
    /// Gets the current position of the object after animation updates.
    /// </summary>
    Point2 WorldPosition { get; }

    /// <summary>
    /// Gets the initial rotation in degrees when animations start.
    /// </summary>
    float StartRotation { get; }

    /// <summary>
    /// Gets the current rotation in degrees after animation updates.
    /// </summary>
    float WorldRotation { get; }

    /// <summary>
    /// Called after animations are applied to update the object state.
    /// </summary>
    /// <param name="startPosition">The starting position of the animation.</param>
    /// <param name="position">The current position after animation.</param>
    /// <param name="startRotation">The starting rotation in degrees.</param>
    /// <param name="rotation">The current rotation in degrees after animation.</param>
    void OnAnimated(Point2 startPosition, Point2 position, float startRotation, float rotation);
}
