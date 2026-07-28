using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Meshes.Internal;

namespace Sachssoft.Sasogine.Graphics.Meshes;

/// <summary>
/// Provides methods for creating standard GPU meshes.
/// </summary>
public static class MeshGenerator
{
    /// <summary>
    /// Creates a textured quad mesh.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device used to create GPU resources.
    /// </param>
    /// <param name="size">
    /// The size of the quad.
    /// </param>
    /// <param name="centerOrigin">
    /// Determines whether the quad is centered around the origin.
    /// If false, the quad uses a top-left origin starting at (0,0).
    /// </param>
    /// <returns>
    /// A GPU mesh containing position, color and texture coordinates.
    /// </returns>
    public static IMesh CreateQuad(
        GraphicsDevice graphicsDevice,
        float size = 1f,
        bool centerOrigin = false)
    {
        // QuadMesh ist die einzige Ausnahme unter den 2D-Shape-Meshes,
        // da ein Quad häufig als allgemeine Renderfläche für Texturen,
        // Shader-Effekte, Debug-Darstellung und RenderTarget-Ausgaben benötigt wird.
        //
        // Standardmäßig verwendet das Quad eine Top-Left-basierte Positionierung,
        // da diese für 2D-Rendering einfacher ist und direkt mit Pixel- und
        // Weltkoordinaten übereinstimmt.
        //
        // Mit centerOrigin kann optional ein Mittelpunkt-Ursprung verwendet werden,
        // was für Rotation oder spezielle Transformationsfälle praktisch ist.
        //
        // Andere 2D-Shapes werden dynamisch über ShapeBatch erzeugt.
        //
        // Für die Darstellung vieler 2D-Formen bitte ShapeBatch oder FrameBatch
        // (nur Rechtecke) verwenden, da ein einzelnes QuadMesh einen eigenen DrawCall verursacht.

        return new QuadMesh(
            graphicsDevice,
            size,
            centerOrigin);
    }


    /// <summary>
    /// Creates a textured cube mesh.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device used to create GPU resources.
    /// </param>
    /// <param name="size">
    /// The size of the cube.
    /// </param>
    /// <returns>
    /// A GPU mesh containing position and texture coordinates.
    /// </returns>
    public static IMesh CreateCube(
        GraphicsDevice graphicsDevice,
        float size = 1f)
    {
        return new TextureCubeMesh(
            graphicsDevice,
            size);
    }


    /// <summary>
    /// Creates a textured sphere mesh.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device used to create GPU resources.
    /// </param>
    /// <param name="radius">
    /// The radius of the sphere.
    /// </param>
    /// <param name="segments">
    /// The horizontal subdivision count.
    /// </param>
    /// <param name="rings">
    /// The vertical subdivision count.
    /// </param>
    /// <returns>
    /// A GPU mesh containing position and texture coordinates.
    /// </returns>
    public static IMesh CreateSphere(
        GraphicsDevice graphicsDevice,
        float radius = 0.5f,
        int segments = 32,
        int rings = 16)
    {
        return new TextureSphereMesh(
            graphicsDevice,
            radius,
            segments,
            rings);
    }


    /// <summary>
    /// Creates a skybox cube mesh with inward-facing faces.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device used to create GPU resources.
    /// </param>
    /// <param name="size">
    /// The size of the skybox cube.
    /// </param>
    /// <returns>
    /// A GPU mesh for skybox rendering.
    /// </returns>
    public static IMesh CreateSkybox(
        GraphicsDevice graphicsDevice,
        float size = 1f)
    {
        // Skybox verwendet einen Würfel statt einer Kugel,
        // da eine Kugel an den Polen Texturverzerrungen erzeugt.
        // Der Würfel bietet eine bessere Abbildung für Cubemap-Texturen.

        return new TextureSkyboxMesh(
            graphicsDevice,
            size);
    }
}