using System;

namespace Sachssoft.Sasogine.Geometry
{
    /// <summary>
    /// Defines options used when simplifying polygon geometry.
    /// </summary>
    public sealed class PolygonSimplificationOptions
    {
        /// <summary>
        /// Gets the tolerance used to determine how much geometric detail
        /// may be removed during simplification.
        /// </summary>
        /// <remarks>
        /// Higher values generally produce simpler geometry with fewer points,
        /// while lower values preserve more of the original shape.
        /// </remarks>
        public float Tolerance { get; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="PolygonSimplificationOptions"/> class.
        /// </summary>
        /// <param name="tolerance">
        /// The tolerance used during polygon simplification.
        /// Must be greater than or equal to <c>0</c>.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="tolerance"/> is negative.
        /// </exception>
        public PolygonSimplificationOptions(float tolerance = 0.01f)
        {
            if (tolerance < 0f)
                throw new ArgumentOutOfRangeException(nameof(tolerance));

            Tolerance = tolerance;
        }
    }
}