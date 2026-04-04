using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Presentation.Layouts
{
    /// <summary>
    /// Einfache Transformation: Translation + Scale, immutable, Matrix gecached.
    /// </summary>
    public readonly struct Transform
    {
        private readonly Vector2 _translation;
        private readonly Vector2 _scale;
        private readonly Matrix _matrix; // cached

        public static readonly Transform Identity = new Transform(Vector2.Zero, Vector2.One);

        public Vector2 Translation => _translation;

        public Vector2 Scale => _scale;

        public Matrix Matrix => _matrix;

        public Transform(Vector2 translation, Vector2 scale)
        {
            _translation = translation;
            _scale = scale;

            // Einfach: zuerst Scale, dann Translation
            _matrix = Matrix.CreateScale(new Vector3(scale, 1f)) *
                      Matrix.CreateTranslation(new Vector3(translation, 0f));
        }

        public Transform WithTranslation(Vector2 translation) => new Transform(translation, _scale);

        public Transform WithScale(Vector2 scale) => new Transform(_translation, scale);
    }
}