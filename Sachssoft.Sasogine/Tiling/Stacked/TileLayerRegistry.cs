using System;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Tiling.Stacked;

public class TileLayerRegistry : IReadOnlyList<ITileLayer>
{
    private readonly ITileLayer[] _layers;

    public TileLayerRegistry(int layer_count, Func<int, ITileLayer> factory)
    {
        if (layer_count <= 0)
            throw new ArgumentOutOfRangeException(nameof(layer_count));

        _layers = new ITileLayer[layer_count];
        var seen = new HashSet<ITileLayer>();

        for (int i = 0; i < layer_count; i++)
        {
            var layer = factory(i);
            ValidateLayer(layer, i, seen);

            _layers[i] = layer;
        }
    }

    public TileLayerRegistry(params ITileLayer[] layers)
    {
        if (layers == null)
            throw new ArgumentNullException(nameof(layers));

        var seen = new HashSet<ITileLayer>();

        for (int i = 0; i < layers.Length; i++)
        {
            var layer = layers[i];
            ValidateLayer(layer, i, seen);
        }

        _layers = (ITileLayer[])layers.Clone();
    }

    private static void ValidateLayer(ITileLayer layer, int expected_index, HashSet<ITileLayer> seen)
    {
        if (layer == null)
            throw new ArgumentNullException(nameof(layer), $"Layer at index {expected_index} is null.");

        if (!seen.Add(layer))
            throw new ArgumentException($"Duplicate ITileLayer instance detected at index {expected_index}.");

        if (layer.Index != expected_index)
            throw new ArgumentException($"ITileLayer index mismatch: expected {expected_index}, got {layer.Index}.");
    }

    public int Count => _layers.Length;

    public ITileLayer this[int index]
    {
        get
        {
            if (index < 0 || index >= _layers.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _layers[index];
        }
    }

    public bool TryGetIndex(ITileLayer layer, out int index)
    {
        index = IndexOf(layer);
        return index >= 0;
    }

    public int IndexOf(ITileLayer layer)
    {
        if (layer == null)
            throw new ArgumentNullException(nameof(layer));

        for (int i = 0; i < _layers.Length; i++)
        {
            if (ReferenceEquals(_layers[i], layer))
                return i;
        }

        return -1;
    }

    IEnumerator<ITileLayer> IEnumerable<ITileLayer>.GetEnumerator() => ((IEnumerable<ITileLayer>)_layers).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _layers.GetEnumerator();

    public override string ToString()
    {
        return $"TileLayerRegistry [Count={_layers.Length}]";
    }

}
