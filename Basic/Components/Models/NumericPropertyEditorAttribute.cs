using System;

namespace Sachssoft.Sasogine.Components.Models
{
    /// <summary>
    /// Defines numeric editor settings for a property.
    /// Supports different numeric property types by storing values as objects
    /// and converting them according to the property's actual type at runtime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class NumericPropertyEditorAttribute : PropertyEditorAttribute
    {
        private NumericPropertyEditorAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance for a floating-point numeric property.
        /// </summary>
        /// <param name="smallChange">
        /// The small increment/decrement step.
        /// </param>
        /// <param name="largeChange">
        /// The large increment/decrement step.
        /// Must not be smaller than <paramref name="smallChange"/>.
        /// </param>
        public NumericPropertyEditorAttribute(
            float smallChange,
            float largeChange)
        {
            ValidateChanges(smallChange, largeChange);

            NumberType = typeof(float);
            SmallChange = smallChange;
            LargeChange = largeChange;
        }

        /// <summary>
        /// Initializes a new instance for a floating-point numeric property
        /// with minimum and maximum bounds.
        /// </summary>
        /// <param name="minimum">
        /// The minimum allowed value.
        /// </param>
        /// <param name="maximum">
        /// The maximum allowed value.
        /// Must not be smaller than <paramref name="minimum"/>.
        /// </param>
        /// <param name="smallChange">
        /// The small increment/decrement step.
        /// </param>
        /// <param name="largeChange">
        /// The large increment/decrement step.
        /// Must not be smaller than <paramref name="smallChange"/>.
        /// </param>
        public NumericPropertyEditorAttribute(
            float minimum,
            float maximum,
            float smallChange,
            float largeChange)
        {
            ValidateBounds(minimum, maximum);
            ValidateChanges(smallChange, largeChange);

            NumberType = typeof(float);
            Minimum = minimum;
            Maximum = maximum;
            SmallChange = smallChange;
            LargeChange = largeChange;
        }

        /// <summary>
        /// Initializes a new instance for an integer numeric property.
        /// </summary>
        /// <param name="smallChange">
        /// The small increment/decrement step.
        /// </param>
        /// <param name="largeChange">
        /// The large increment/decrement step.
        /// Must not be smaller than <paramref name="smallChange"/>.
        /// </param>
        public NumericPropertyEditorAttribute(
            int smallChange,
            int largeChange)
        {
            ValidateChanges(smallChange, largeChange);

            NumberType = typeof(int);
            SmallChange = smallChange;
            LargeChange = largeChange;
        }

        /// <summary>
        /// Initializes a new instance for an integer numeric property
        /// with minimum and maximum bounds.
        /// </summary>
        /// <param name="minimum">
        /// The minimum allowed value.
        /// </param>
        /// <param name="maximum">
        /// The maximum allowed value.
        /// Must not be smaller than <paramref name="minimum"/>.
        /// </param>
        /// <param name="smallChange">
        /// The small increment/decrement step.
        /// </param>
        /// <param name="largeChange">
        /// The large increment/decrement step.
        /// Must not be smaller than <paramref name="smallChange"/>.
        /// </param>
        public NumericPropertyEditorAttribute(
            int minimum,
            int maximum,
            int smallChange,
            int largeChange)
        {
            ValidateBounds(minimum, maximum);
            ValidateChanges(smallChange, largeChange);

            NumberType = typeof(int);
            Minimum = minimum;
            Maximum = maximum;
            SmallChange = smallChange;
            LargeChange = largeChange;
        }

        /// <summary>
        /// Gets the numeric type handled by this editor configuration.
        /// </summary>
        public Type NumberType { get; }

        /// <summary>
        /// Gets the minimum allowed value.
        /// Returns <c>null</c> if no lower bound is defined.
        /// </summary>
        public object? Minimum { get; }

        /// <summary>
        /// Gets the maximum allowed value.
        /// Returns <c>null</c> if no upper bound is defined.
        /// </summary>
        public object? Maximum { get; }

        /// <summary>
        /// Gets the small increment/decrement step.
        /// </summary>
        public object SmallChange { get; }

        /// <summary>
        /// Gets the large increment/decrement step.
        /// </summary>
        public object LargeChange { get; }

        /// <summary>
        /// Gets or sets the display format string.
        /// </summary>
        public string? Format { get; init; }

        /// <summary>
        /// Gets a value indicating whether minimum and maximum bounds are defined.
        /// </summary>
        public bool HasBounds =>
            Minimum != null && Maximum != null;

        /// <summary>
        /// Validates that the maximum value is not smaller than the minimum value.
        /// </summary>
        /// <typeparam name="T">
        /// The comparable numeric type.
        /// </typeparam>
        /// <param name="minimum">
        /// The minimum value.
        /// </param>
        /// <param name="maximum">
        /// The maximum value.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when maximum is smaller than minimum.
        /// </exception>
        private static void ValidateBounds<T>(
            T minimum,
            T maximum)
            where T : IComparable<T>
        {
            if (maximum.CompareTo(minimum) < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximum),
                    maximum,
                    "Maximum value cannot be smaller than minimum value.");
            }
        }

        /// <summary>
        /// Validates the increment/decrement step values.
        /// Ensures that changes are non-negative and that the large change
        /// is not smaller than the small change.
        /// </summary>
        /// <typeparam name="T">
        /// The comparable numeric type.
        /// </typeparam>
        /// <param name="smallChange">
        /// The small step value.
        /// </param>
        /// <param name="largeChange">
        /// The large step value.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when a change value is negative or largeChange is smaller
        /// than smallChange.
        /// </exception>
        private static void ValidateChanges<T>(
            T smallChange,
            T largeChange)
            where T : IComparable<T>
        {
            if (smallChange.CompareTo(default!) < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(smallChange),
                    smallChange,
                    "Small change cannot be negative.");
            }

            if (largeChange.CompareTo(smallChange) < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(largeChange),
                    largeChange,
                    "Large change cannot be smaller than small change.");
            }
        }
    }
}