namespace Sachssoft.Engine.Controls;

/// <summary>Shares affine coordinate conversion between drawing, hit testing and editor gestures.</summary>
internal static class ControlTransforms
{
    /// <summary>Composes a control's transform with the transforms of its ancestors.</summary>
    /// <param name="control">The control whose layout coordinates are being transformed.</param>
    /// <returns>The matrix mapping absolute layout coordinates to workspace coordinates.</returns>
    internal static Matrix GetVisualTransform(Control control)
    {
        Matrix result = Matrix.Identity;
        for (Control? node = control; node is not null; node = node.Parent)
            if (node is IControlTransform transformed)
                result *= transformed.VisualTransform;
        return result;
    }

    /// <summary>Combines the local transform with the already composed ancestor transform.</summary>
    /// <param name="control">The control being visited.</param>
    /// <param name="parent">The transform of the control's parent.</param>
    /// <returns>The control's transform in workspace coordinates.</returns>
    internal static Matrix Compose(Control control, Matrix parent)
        => control is IControlTransform transformed ? transformed.VisualTransform * parent : parent;

    /// <summary>Recognizes an axis-preserving translation in the UI plane.</summary>
    /// <param name="transform">The affine visual transform.</param>
    /// <param name="offset">The logical displacement when the transform is a translation.</param>
    /// <returns>Whether the transform changes only the X and Y origin.</returns>
    internal static bool TryGetTranslation(Matrix transform, out Vector2 offset)
    {
        offset = new Vector2(transform.M41, transform.M42);
        transform.M41 = 0;
        transform.M42 = 0;
        return transform == Matrix.Identity;
    }

    /// <summary>Calculates an axis-aligned envelope of a transformed layout rectangle.</summary>
    /// <param name="bounds">The untransformed rectangle.</param>
    /// <param name="transform">The matrix mapping layout to workspace coordinates.</param>
    /// <returns>The rectangle enclosing the transformed corners.</returns>
    internal static Bounds2 TransformBounds(Bounds2 bounds, Matrix transform)
    {
        if (transform == Matrix.Identity)
            return bounds;
        if (TryGetTranslation(transform, out Vector2 offset))
            return new Bounds2(bounds.X + offset.X, bounds.Y + offset.Y, bounds.Width, bounds.Height);

        Vector2 a = Vector2.Transform(new Vector2(bounds.Left, bounds.Top), transform);
        Vector2 b = Vector2.Transform(new Vector2(bounds.Right, bounds.Top), transform);
        Vector2 c = Vector2.Transform(new Vector2(bounds.Left, bounds.Bottom), transform);
        Vector2 d = Vector2.Transform(new Vector2(bounds.Right, bounds.Bottom), transform);
        float x = MathF.Min(MathF.Min(a.X, b.X), MathF.Min(c.X, d.X));
        float y = MathF.Min(MathF.Min(a.Y, b.Y), MathF.Min(c.Y, d.Y));
        float right = MathF.Max(MathF.Max(a.X, b.X), MathF.Max(c.X, d.X));
        float bottom = MathF.Max(MathF.Max(a.Y, b.Y), MathF.Max(c.Y, d.Y));
        return new Bounds2(x, y, right - x, bottom - y);
    }

    /// <summary>Converts a workspace pointer position back into a control's layout coordinate space.</summary>
    /// <param name="control">The receiving control.</param>
    /// <param name="position">The pointer position in workspace coordinates.</param>
    /// <returns>The equivalent absolute untransformed layout position.</returns>
    internal static Vector2 ToLayout(Control control, Vector2 position)
    {
        Matrix transform = GetVisualTransform(control);
        return transform == Matrix.Identity ? position : Vector2.Transform(position, Matrix.Invert(transform));
    }
}
