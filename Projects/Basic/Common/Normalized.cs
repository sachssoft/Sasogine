using Sachssoft.Sasogine.Geometry;
using System;
using System.Globalization;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Represents a normalized floating-point value constrained to the range
    /// from 0 to 1.
    /// </summary>
    public readonly struct Normalized :
        IEquatable<Normalized>,
        IComparable<Normalized>
    {
        private readonly float _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Normalized"/> struct.
        ///
        /// Values outside the normalized range are clamped to 0 or 1.
        /// <see cref="float.NaN"/> is normalized to 0.
        /// </summary>
        /// <param name="value">Value to normalize.</param>
        public Normalized(float value)
        {
            _value = float.IsNaN(value)
                ? 0f
                : Math.Clamp(value, 0f, 1f);
        }

        /// <summary>
        /// Converts a floating-point value to a normalized value.
        /// </summary>
        public static implicit operator Normalized(float value)
        {
            return new Normalized(value);
        }

        /// <summary>
        /// Converts a normalized value to its floating-point representation.
        /// </summary>
        public static implicit operator float(Normalized value)
        {
            return value._value;
        }

        /// <summary>
        /// Adds two normalized values and clamps the result to the normalized range.
        /// </summary>
        public static Normalized operator +(Normalized a, Normalized b)
        {
            return new Normalized(a._value + b._value);
        }

        /// <summary>
        /// Subtracts two normalized values and clamps the result to the normalized range.
        /// </summary>
        public static Normalized operator -(Normalized a, Normalized b)
        {
            return new Normalized(a._value - b._value);
        }

        /// <summary>
        /// Multiplies two normalized values.
        /// </summary>
        public static Normalized operator *(Normalized a, Normalized b)
        {
            return new Normalized(a._value * b._value);
        }

        /// <summary>
        /// Divides one normalized value by another and clamps the result
        /// to the normalized range.
        /// </summary>
        public static Normalized operator /(Normalized a, Normalized b)
        {
            return new Normalized(a._value / b._value);
        }

        /// <summary>
        /// Determines whether two normalized values are equal.
        /// </summary>
        public static bool operator ==(Normalized a, Normalized b)
        {
            return a._value == b._value;
        }

        /// <summary>
        /// Determines whether two normalized values are not equal.
        /// </summary>
        public static bool operator !=(Normalized a, Normalized b)
        {
            return a._value != b._value;
        }

        /// <summary>
        /// Determines whether the left value is less than the right value.
        /// </summary>
        public static bool operator <(Normalized a, Normalized b)
        {
            return a._value < b._value;
        }

        /// <summary>
        /// Determines whether the left value is greater than the right value.
        /// </summary>
        public static bool operator >(Normalized a, Normalized b)
        {
            return a._value > b._value;
        }

        /// <summary>
        /// Determines whether the left value is less than or equal to the right value.
        /// </summary>
        public static bool operator <=(Normalized a, Normalized b)
        {
            return a._value <= b._value;
        }

        /// <summary>
        /// Determines whether the left value is greater than or equal to the right value.
        /// </summary>
        public static bool operator >=(Normalized a, Normalized b)
        {
            return a._value >= b._value;
        }

        /// <summary>
        /// Returns a string representation of the normalized value.
        /// </summary>
        public override string ToString()
        {
            return _value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Determines whether this value equals another normalized value.
        /// </summary>
        public bool Equals(Normalized other)
        {
            return _value.Equals(other._value);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is Normalized other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        /// <summary>
        /// Compares this value with another normalized value.
        /// </summary>
        public int CompareTo(Normalized other)
        {
            return _value.CompareTo(other._value);
        }
    }
}