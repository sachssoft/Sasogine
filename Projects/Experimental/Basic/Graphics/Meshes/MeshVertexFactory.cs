using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Sasogine.Experimental.Graphics.Meshes;

/// <summary>
/// Creates a concrete GPU vertex from generated mesh vertex data.
/// </summary>
public delegate TVertex MeshVertexFactory<TVertex>(in MeshVertexData data)
    where TVertex : struct, IVertexType;
