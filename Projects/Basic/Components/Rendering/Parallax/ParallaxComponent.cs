using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Rendering;

/// <summary>
/// Provides a parallax rendering component that manages multiple independent parallax layers.
/// </summary>
public class ParallaxComponent : ResourceComponentBase, IDrawableComponent
{
    private readonly List<IParallaxLayerComponent> _layers = [];

    /// <summary>
    /// Gets the parallax layers managed by this component.
    /// </summary>
    public IList<IParallaxLayerComponent> Layers => _layers;

    /// <summary>
    /// Gets or sets a value indicating whether the component updates its layers.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the component renders its layers.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Adds a parallax layer.
    /// </summary>
    /// <typeparam name="T">The type of layer to add.</typeparam>
    /// <param name="layer">The layer to add.</param>
    /// <returns>The added layer.</returns>
    public T AddLayer<T>(T layer) where T : class, IParallaxLayerComponent
    {
        ArgumentNullException.ThrowIfNull(layer);

        for (int i = 0; i < _layers.Count; i++)
        {
            if (ReferenceEquals(_layers[i], layer))
                throw new InvalidOperationException("The parallax layer is already registered.");
        }

        _layers.Add(layer);
        return layer;
    }

    /// <summary>
    /// Removes a parallax layer.
    /// </summary>
    /// <param name="layer">The layer to remove.</param>
    /// <returns><see langword="true"/> if the layer was removed; otherwise <see langword="false"/>.</returns>
    public bool RemoveLayer(IParallaxLayerComponent layer)
    {
        ArgumentNullException.ThrowIfNull(layer);
        return _layers.Remove(layer);
    }

    /// <summary>
    /// Removes all parallax layers.
    /// </summary>
    public void ClearLayers() => _layers.Clear();

    /// <summary>
    /// Resets all registered parallax layers.
    /// </summary>
    public void Reset()
    {
        for (int i = 0; i < _layers.Count; i++)
            _layers[i].Reset();
    }

    /// <summary>
    /// Updates all enabled parallax layers.
    /// </summary>
    /// <param name="context">Provides scene update information.</param>
    public override void Update(SceneUpdateContext context)
    {
        if (!IsEnabled)
            return;

        for (int i = 0; i < _layers.Count; i++)
        {
            if (_layers[i].IsEnabled)
                _layers[i].Update(context);
        }
    }

    /// <summary>
    /// Draws all visible parallax layers in ascending draw order.
    /// </summary>
    /// <param name="context">Provides scene rendering information.</param>
    public void Draw(SceneDrawContext context)
    {
        if (!IsVisible)
            return;

        _layers.Sort(static (a, b) => a.Index.CompareTo(b.Index));

        for (int i = 0; i < _layers.Count; i++)
        {
            if (_layers[i].IsVisible)
                _layers[i].Draw(context);
        }
    }
}
