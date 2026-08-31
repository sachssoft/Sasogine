using Sachssoft.Sasogine.Geometry;
using System;
using System.Globalization;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Represents a signed normalized floating-point value constrained
    /// to the range from -1 to 1.
    /// </summary>
    public readonly struct SignedNormalized :
        IEquatable<SignedNormalized>,
        IComparable<SignedNormalized>
    {
        private readonly float _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignedNormalized"/> struct.
        ///
        /// Values outside the signed normalized range are clamped to -1 or 1.
        /// <see cref="float.NaN"/> is normalized to 0.
        /// </summary>
        /// <param name="value">Value to normalize.</param>
        public SignedNormalized(float value)
        {
            _value = float.IsNaN(value)
                ? 0f
                : Math.Clamp(value, -1f, 1f);
        }

        /// <summary>
        /// Converts a floating-point value to a signed normalized value.
        /// </summary>
        public static implicit operator SignedNormalized(float value)
        {
            return new SignedNormalized(value);
        }

        /// <summary>
        /// Converts a signed normalized value to its floating-point representation.
        /// </summary>
        public static implicit operator float(SignedNormalized value)
        {
            return value._value;
        }

        /// <summary>
        /// Adds two signed normalized values and clamps the result.
        /// </summary>
        public static SignedNormalized operator +(SignedNormalized a, SignedNormalized b)
        {
            return new SignedNormalized(a._value + b._value);
        }

        /// <summary>
        /// Subtracts two signed normalized values and clamps the result.
        /// </summary>
        public static SignedNormalized operator -(SignedNormalized a, SignedNormalized b)
        {
            return new SignedNormalized(a._value - b._value);
        }

        /// <summary>
        /// Multiplies two signed normalized values.
        /// </summary>
        public static SignedNormalized operator *(SignedNormalized a, SignedNormalized b)
        {
            return new SignedNormalized(a._value * b._value);
        }

        /// <summary>
        /// Divides one signed normalized value by another and clamps the result.
        /// </summary>
        public static SignedNormalized operator /(SignedNormalized a, SignedNormalized b)
        {
            return new SignedNormalized(a._value / b._value);
        }

        /// <summary>
        /// Determines whether two signed normalized values are equal.
        /// </summary>
        public static bool operator ==(SignedNormalized a, SignedNormalized b)
        {
            return a._value == b._value;
        }

        /// <summary>
        /// Determines whether two signed normalized values are not equal.
        /// </summary>
        public static bool operator !=(SignedNormalized a, SignedNormalized b)
        {
            return a._value != b._value;
        }

        /// <summary>
        /// Determines whether the left value is less than the right value.
        /// </summary>
        public static bool operator <(SignedNormalized a, SignedNormalized b)
        {
            return a._value < b._value;
        }

        /// <summary>
        /// Determines whether the left value is greater than the right value.
        /// </summary>
        public static bool operator >(SignedNormalized a, SignedNormalized b)
        {
            return a._value > b._value;
        }

        /// <summary>
        /// Determines whether the left value is less than or equal to the right value.
        /// </summary>
        public static bool operator <=(SignedNormalized a, SignedNormalized b)
        {
            return a._value <= b._value;
        }

        /// <summary>
        /// Determines whether the left value is greater than or equal to the right value.
        /// </summary>
        public static bool operator >=(SignedNormalized a, SignedNormalized b)
        {
            return a._value >= b._value;
        }

        /// <summary>
        /// Returns a string representation of the signed normalized value.
        /// </summary>
        public override string ToString()
        {
            return _value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Determines whether this value equals another signed normalized value.
        /// </summary>
        public bool Equals(SignedNormalized other)
        {
            return _value.Equals(other._value);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is SignedNormalized other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        /// <summary>
        /// Compares this value with another signed normalized value.
        /// </summary>
        public int CompareTo(SignedNormalized other)
        {
            return _value.CompareTo(other._value);
        }
    }
}