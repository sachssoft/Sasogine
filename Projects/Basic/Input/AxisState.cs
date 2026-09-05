using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Represents two-dimensional input values for enum-based axes.
    /// </summary>
    /// <typeparam name="TAxis">
    /// The enum type used to identify axes.
    /// </typeparam>
    public sealed class Axis<TAxis>
        where TAxis : unmanaged, Enum
    {
        private readonly Vector2[] _values;

        /// <summary>
        /// Initializes a new instance of the <see cref="Axis{TAxis}"/> class.
        /// </summary>
        /// <param name="maxAxisValue">
        /// The maximum numeric axis value that can be stored.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="maxAxisValue"/> is negative.
        /// </exception>
        public Axis(int maxAxisValue = 511)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(maxAxisValue);
            _values = new Vector2[maxAxisValue + 1];
        }

        /// <summary>
        /// Sets the value of the specified axis.
        /// </summary>
        public void Set(TAxis axis, Vector2 value)
        {
            var index = EnumInteractionConverter<TAxis>.ToUInt64(axis);

            if (index < (ulong)_values.Length)
                _values[(int)index] = value;
        }

        /// <summary>
        /// Gets the value of the specified axis.
        /// </summary>
        public Vector2 Get(TAxis axis)
        {
            var index = EnumInteractionConverter<TAxis>.ToUInt64(axis);

            return index < (ulong)_values.Length
                ? _values[(int)index]
                : Vector2.Zero;
        }

        /// <summary>
        /// Invokes the specified action for each axis with a non-zero value.
        /// </summary>
        public void ForEachNonZero(Action<TAxis, Vector2> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            for (int i = 0; i < _values.Length; i++)
            {
                var value = _values[i];

                if (value == Vector2.Zero)
                    continue;

                action(EnumInteractionConverter<TAxis>.FromUInt64((ulong)i), value);
            }
        }

        /// <summary>
        /// Resets all axis values to zero.
        /// </summary>
        public void Clear()
        {
            Array.Clear(_values);
        }
    }
}