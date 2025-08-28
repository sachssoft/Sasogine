using Microsoft.Xna.Framework;
using Sachssoft.Observables;

namespace Sachssoft.Sasogine.Graphics
{
    /// <summary>
    /// Represents a 2D render transformation including translation, rotation, scale, and origin.
    /// Implements <see cref="ITransform"/> and exposes properties as <see cref="IProperty"/> for binding.
    /// </summary>
    public class RenderTransform : NotifyingObject, ITransform
    {
        public static readonly IProperty TranslationProperty =
            new StoredProperty<RenderTransform, Vector2>(
                nameof(Translation),
                defaultValue: Vector2.Zero,
                category: Sachssoft.Sasogine.Observables.PropertyCategories.Transform);

        /// <summary>
        /// Gets or sets the translation vector.
        /// </summary>
        public Vector2 Translation
        {
            get => GetValue<Vector2>(TranslationProperty);
            set => SetValue(TranslationProperty, value);
        }

        public static readonly IProperty RotationProperty =
            new StoredProperty<RenderTransform, float>(
                nameof(Rotation),
                defaultValue: 0f,
                category: Sachssoft.Sasogine.Observables.PropertyCategories.Transform);

        /// <summary>
        /// Gets or sets the rotation in radians.
        /// </summary>
        public float Rotation
        {
            get => GetValue<float>(RotationProperty);
            set => SetValue(RotationProperty, value);
        }

        public static readonly IProperty ScaleProperty =
            new StoredProperty<RenderTransform, Vector2>(
                nameof(Scale),
                defaultValue: Vector2.One,
                category: Sachssoft.Sasogine.Observables.PropertyCategories.Transform);

        /// <summary>
        /// Gets or sets the scale vector.
        /// </summary>
        public Vector2 Scale
        {
            get => GetValue<Vector2>(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        public static readonly IProperty OriginProperty =
            new StoredProperty<RenderTransform, Vector2>(
                nameof(Origin),
                defaultValue: new Vector2(0.5f),
                category: Sachssoft.Sasogine.Observables.PropertyCategories.Transform);

        /// <summary>
        /// Gets or sets the origin vector for rotation and scaling.
        /// </summary>
        public Vector2 Origin
        {
            get => GetValue<Vector2>(OriginProperty);
            set => SetValue(OriginProperty, value);
        }

        /// <summary>
        /// Converts this transform to a <see cref="Matrix"/> including translation, rotation, scale, and origin.
        /// </summary>
        /// <returns>The resulting transformation matrix.</returns>
        public Matrix ToMatrix()
        {
            return MatrixHelper.Create(Translation, new Vector2(1f / Scale.X, 1f / Scale.Y), Rotation, Origin);
        }
    }
}
