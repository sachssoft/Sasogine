using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Graphics.Meshes;

/// <summary>
/// Verwaltet ein unverändertes Basis-Mesh und optionale, unabhängige Zusatz-Meshes.
/// </summary>
/// <remarks>
/// Diese Klasse verändert weder die bestehende Path-Geometrie noch
/// die vorhandenen Methoden von <see cref="MeshGenerator"/>.
/// Alle übergebenen Meshes werden übernommen und beim Entfernen bzw. beim Dispose
/// freigegeben. Renderreihenfolge, Shader und Renderzustände bestimmt der Aufrufer.
/// </remarks>
public sealed class LayeredPolygonMesh : IDisposable
{
    private readonly IMesh _baseMesh;
    private readonly List<PolygonMeshLayer> _layers = new();
    private readonly IReadOnlyList<PolygonMeshLayer> _readOnlyLayers;
    private bool _disposed;

    /// <summary>
    /// Initialisiert die Mesh-Komposition mit der ursprünglichen Polygongeometrie.
    /// </summary>
    /// <param name="baseMesh">Das unveränderte Basis-Mesh. Die Ownership wird übernommen.</param>
    public LayeredPolygonMesh(IMesh baseMesh)
    {
        ArgumentNullException.ThrowIfNull(baseMesh);

        _baseMesh = baseMesh;
        _readOnlyLayers = _layers.AsReadOnly();
    }

    /// <summary>
    /// Ruft das ursprüngliche Basis-Mesh ab.
    /// </summary>
    public IMesh BaseMesh
    {
        get
        {
            ThrowIfDisposed();
            return _baseMesh;
        }
    }

    /// <summary>
    /// Ruft die Zusatz-Layer in ihrer Einfügereihenfolge ab.
    /// </summary>
    public IReadOnlyList<PolygonMeshLayer> Layers
    {
        get
        {
            ThrowIfDisposed();
            return _readOnlyLayers;
        }
    }

    /// <summary>
    /// Ruft die Anzahl der zusätzlichen Mesh-Layer ab.
    /// </summary>
    public int LayerCount
    {
        get
        {
            ThrowIfDisposed();
            return _layers.Count;
        }
    }

    /// <summary>
    /// Fügt ein unabhängiges Mesh als benanntes Layer hinzu.
    /// </summary>
    /// <remarks>
    /// Nach erfolgreichem Hinzufügen darf das Mesh nicht mehr separat entsorgt werden.
    /// Layer-Namen sind innerhalb dieser Komposition eindeutig (Ordinal-Vergleich).
    /// </remarks>
    public PolygonMeshLayer AddLayer(string name, IMesh mesh)
    {
        ThrowIfDisposed();
        ValidateName(name);
        ArgumentNullException.ThrowIfNull(mesh);

        if (ContainsLayer(name))
            throw new ArgumentException($"A layer named '{name}' already exists.", nameof(name));

        if (ReferenceEquals(mesh, _baseMesh))
            throw new ArgumentException("The base mesh cannot also be a layer.", nameof(mesh));

        foreach (PolygonMeshLayer layer in _layers)
        {
            if (ReferenceEquals(layer.Mesh, mesh))
                throw new ArgumentException("The mesh is already registered as a layer.", nameof(mesh));
        }

        var result = new PolygonMeshLayer(name, mesh);
        _layers.Add(result);
        return result;
    }

    /// <summary>
    /// Prüft, ob bereits ein Layer mit diesem Namen existiert.
    /// </summary>
    public bool ContainsLayer(string name)
    {
        ThrowIfDisposed();
        ValidateName(name);
        return FindLayerIndex(name) >= 0;
    }

    /// <summary>
    /// Sucht ein Layer anhand seines Namens.
    /// </summary>
    public bool TryGetLayer(string name, out PolygonMeshLayer? layer)
    {
        ThrowIfDisposed();
        ValidateName(name);

        int index = FindLayerIndex(name);
        layer = index >= 0 ? _layers[index] : null;
        return index >= 0;
    }

    /// <summary>
    /// Entfernt ein Layer und gibt sein GPU-Mesh frei.
    /// </summary>
    public bool RemoveLayer(string name)
    {
        ThrowIfDisposed();
        ValidateName(name);

        int index = FindLayerIndex(name);
        if (index < 0)
            return false;

        IMesh mesh = _layers[index].Mesh;
        _layers.RemoveAt(index);
        mesh.Dispose();
        return true;
    }

    /// <summary>
    /// Entfernt ein Layer, ohne dessen Mesh freizugeben.
    /// Die Ownership geht an den Aufrufer zurück.
    /// </summary>
    public IMesh? DetachLayer(string name)
    {
        ThrowIfDisposed();
        ValidateName(name);

        int index = FindLayerIndex(name);
        if (index < 0)
            return null;

        IMesh mesh = _layers[index].Mesh;
        _layers.RemoveAt(index);
        return mesh;
    }

    /// <summary>
    /// Entfernt sämtliche zusätzlichen Layer und gibt deren GPU-Meshes frei.
    /// Das ursprüngliche Basis-Mesh bleibt bestehen.
    /// </summary>
    public void ClearLayers()
    {
        ThrowIfDisposed();

        for (int i = _layers.Count - 1; i >= 0; i--)
        {
            IMesh mesh = _layers[i].Mesh;
            _layers.RemoveAt(i);
            mesh.Dispose();
        }
    }

    /// <summary>
    /// Gibt das Basis-Mesh und alle noch registrierten Zusatz-Meshes frei.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        for (int i = _layers.Count - 1; i >= 0; i--)
            _layers[i].Mesh.Dispose();

        _layers.Clear();
        _baseMesh.Dispose();
    }

    private int FindLayerIndex(string name)
    {
        for (int i = 0; i < _layers.Count; i++)
        {
            if (string.Equals(_layers[i].Name, name, StringComparison.Ordinal))
                return i;
        }

        return -1;
    }

    private static void ValidateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(LayeredPolygonMesh));
    }
}
