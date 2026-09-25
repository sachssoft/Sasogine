using Sachssoft.Engine;
using Sachssoft.Engine.Performance;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Components.Definitions;

/// <summary>
/// Represents a strongly typed property of a definition.
/// </summary>
/// <typeparam name="TDefinition">
/// The definition type that declares or provides the property.
/// </typeparam>
/// <typeparam name="TValue">
/// The value type of the property.
/// </typeparam>
/// <remarks>
/// Provides property metadata, attributes, strongly typed value access,
/// value coercion, validation, and change notification without requiring
/// runtime property reflection, making it suitable for trimming and AOT scenarios.
/// </remarks>
public class DefinitionProperty<TDefinition, TValue> : IDefinitionProperty
    where TDefinition : IDefinition
{
    private readonly Func<TDefinition, TValue> _getter;
    private readonly Action<TDefinition, TValue>? _setter;
    private readonly IReadOnlyList<Attribute> _attributes;
    private readonly Func<TDefinition, TValue, TValue>? _coerceValue;
    private readonly Func<TDefinition, TValue, bool>? _validateValue;
    private readonly Action<TDefinition, TValue, TValue>? _valueChanged;

    private ValueBuffer<TValue> _valueBuffer;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="DefinitionProperty{TDefinition, TValue}"/> class.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="getter">The delegate used to retrieve the property value.</param>
    /// <param name="setter">
    /// The delegate used to assign the property value, or <see langword="null"/>
    /// if the property is read-only.
    /// </param>
    /// <param name="attributes">
    /// The attributes associated with the property, or <see langword="null"/>
    /// if the property has no attributes.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is empty or consists only of white-space characters.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="getter"/> is <see langword="null"/>.
    /// </exception>
    public DefinitionProperty(
        string name,
        Func<TDefinition, TValue> getter,
        Action<TDefinition, TValue>? setter = null,
        IReadOnlyList<Attribute>? attributes = null)
        : this(name, getter, setter, attributes, null, null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="DefinitionProperty{TDefinition, TValue}"/> class with custom
    /// value coercion, validation, and change handling.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="getter">The delegate used to retrieve the property value.</param>
    /// <param name="setter">
    /// The delegate used to assign the property value, or <see langword="null"/>
    /// if the property is read-only.
    /// </param>
    /// <param name="attributes">
    /// The attributes associated with the property, or <see langword="null"/>
    /// if the property has no attributes.
    /// </param>
    /// <param name="coerceValue">
    /// The delegate used to coerce a value before validation and assignment,
    /// or <see langword="null"/> to use the value unchanged.
    /// </param>
    /// <param name="validateValue">
    /// The delegate used to validate a value before assignment,
    /// or <see langword="null"/> to accept all values.
    /// </param>
    /// <param name="valueChanged">
    /// The delegate invoked after the property value has changed,
    /// or <see langword="null"/> if no callback is required.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is empty or consists only of white-space characters.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="getter"/> is <see langword="null"/>.
    /// </exception>
    public DefinitionProperty(
        string name,
        Func<TDefinition, TValue> getter,
        Action<TDefinition, TValue>? setter,
        IReadOnlyList<Attribute>? attributes,
        Func<TDefinition, TValue, TValue>? coerceValue,
        Func<TDefinition, TValue, bool>? validateValue,
        Action<TDefinition, TValue, TValue>? valueChanged)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(getter);

        Name = name;

        _getter = getter;
        _setter = setter;
        _attributes = attributes ?? Array.Empty<Attribute>();
        _coerceValue = coerceValue;
        _validateValue = validateValue;
        _valueChanged = valueChanged;
    }

    /// <inheritdoc/>
    public event EventHandler<DefinitionPropertyChangedEventArgs>? ValueChanged;

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public Type Type => typeof(TValue);

    /// <inheritdoc/>
    public bool IsReadOnly => _setter == null;

    /// <inheritdoc/>
    public IReadOnlyList<Attribute> Attributes => _attributes;

    /// <inheritdoc/>
    public TAttribute? GetAttribute<TAttribute>() where TAttribute : Attribute
    {
        foreach (var attribute in _attributes)
        {
            if (attribute is TAttribute result)
                return result;
        }

        return null;
    }

    /// <inheritdoc/>
    public bool HasAttribute<TAttribute>() where TAttribute : Attribute
    {
        foreach (var attribute in _attributes)
        {
            if (attribute is TAttribute)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the value of the property from the specified definition.
    /// </summary>
    /// <param name="source">The definition from which to retrieve the value.</param>
    /// <returns>The current property value.</returns>
    public TValue GetValue(TDefinition source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return _getter(source);
    }

    /// <inheritdoc/>
    object? IDefinitionProperty.GetValue(IDefinition source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source is not TDefinition definition)
            throw new ArgumentException(
                $"Definition must be of type '{typeof(TDefinition).FullName}'.",
                nameof(source));

        return GetValue(definition);
    }

    /// <summary>
    /// Determines whether the specified value can be assigned to the property.
    /// </summary>
    /// <param name="source">The definition on which the value would be assigned.</param>
    /// <param name="value">The value to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the value can be assigned; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool CanSetValue(TDefinition source, TValue value)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (_setter == null)
            return false;

        var coercedValue = CoerceValue(source, value);
        return ValidateValue(source, coercedValue);
    }

    /// <inheritdoc/>
    bool IDefinitionProperty.CanSetValue(IDefinition source, object? value)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (_setter == null || source is not TDefinition definition)
            return false;

        if (value is TValue typedValue)
            return CanSetValue(definition, typedValue);

        if (value == null && default(TValue) == null)
            return CanSetValue(definition, default!);

        return false;
    }

    /// <summary>
    /// Sets the value of the property on the specified definition.
    /// </summary>
    /// <param name="source">The definition on which to set the value.</param>
    /// <param name="value">The value to assign.</param>
    /// <exception cref="InvalidOperationException">
    /// The property is read-only.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The specified value is not valid for the property.
    /// </exception>
    public void SetValue(TDefinition source, TValue value)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (_setter == null)
            throw new InvalidOperationException($"Property '{Name}' is read-only.");

        var coercedValue = CoerceValue(source, value);

        if (!ValidateValue(source, coercedValue))
            throw new ArgumentException(
                $"Value is not valid for property '{Name}'.",
                nameof(value));

        var oldValue = _getter(source);

        if (EqualityComparer<TValue>.Default.Equals(oldValue, coercedValue))
            return;

        _setter(source, coercedValue);
        _valueBuffer = coercedValue;

        OnValueChanged(source, oldValue, coercedValue);
    }

    /// <inheritdoc/>
    void IDefinitionProperty.SetValue(IDefinition source, object? value)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source is not TDefinition definition)
            throw new ArgumentException(
                $"Definition must be of type '{typeof(TDefinition).FullName}'.",
                nameof(source));

        if (value is TValue typedValue)
        {
            SetValue(definition, typedValue);
            return;
        }

        if (value == null && default(TValue) == null)
        {
            SetValue(definition, default!);
            return;
        }

        throw new ArgumentException(
            $"Value must be of type '{typeof(TValue).FullName}'.",
            nameof(value));
    }

    /// <summary>
    /// Detects whether the current property value has changed since the last check.
    /// </summary>
    /// <param name="source">The definition whose property value is checked.</param>
    /// <returns>
    /// <see langword="true"/> if the value has changed or has not yet been
    /// initialized; otherwise, <see langword="false"/>.
    /// </returns>
    public bool DetectValueChange(TDefinition source)
    {
        ArgumentNullException.ThrowIfNull(source);

        // Der aktuelle Wert wird immer über GetValue abgerufen.
        // Je nach Implementierung stammt dieser entweder direkt aus der Definition
        // oder wird über einen Reflection-basierten Zugriff ausgelesen.
        //
        // Direkte Änderungen an der Definition können hier nicht automatisch erkannt
        // werden, da keine Benachrichtigung wie INotifyPropertyChanged vorausgesetzt wird.
        // Deshalb wird diese Methode regelmäßig während eines Updates aufgerufen
        // und arbeitet damit bewusst nach dem Polling-Prinzip.
        //
        // Der aktuelle Wert wird mit dem zuletzt im ValueBuffer gespeicherten Wert
        // verglichen. Bei der ersten Prüfung oder wenn sich der Wert seit der letzten
        // Prüfung geändert hat, aktualisiert EnsureChange den Buffer und gibt true zurück.

        var currentValue = GetValue(source);
        return _valueBuffer.EnsureChange(currentValue);
    }

    /// <inheritdoc/>
    bool IDefinitionProperty.DetectValueChange(IDefinition source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source is not TDefinition definition)
            throw new ArgumentException(
                $"Definition must be of type '{typeof(TDefinition).FullName}'.",
                nameof(source));

        return DetectValueChange(definition);
    }

    /// <summary>
    /// Coerces a value before it is validated and assigned to the property.
    /// </summary>
    /// <param name="source">The definition on which the value will be assigned.</param>
    /// <param name="value">The proposed value.</param>
    /// <returns>The coerced value.</returns>
    protected virtual TValue CoerceValue(TDefinition source, TValue value)
    {
        if (_coerceValue != null)
            return _coerceValue(source, value);

        return value;
    }

    /// <summary>
    /// Determines whether the specified value is valid for the property.
    /// </summary>
    /// <param name="source">The definition on which the value would be assigned.</param>
    /// <param name="value">The coerced value to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the value is valid; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    protected virtual bool ValidateValue(TDefinition source, TValue value)
    {
        return _validateValue?.Invoke(source, value) ?? true;
    }

    /// <summary>
    /// Called after the property value has changed.
    /// </summary>
    /// <param name="source">The definition whose property value changed.</param>
    /// <param name="oldValue">The previous property value.</param>
    /// <param name="newValue">The new property value.</param>
    /// <remarks>
    /// The configured value-changed callback is invoked before the
    /// <see cref="ValueChanged"/> event is raised.
    /// </remarks>
    protected virtual void OnValueChanged(TDefinition source, TValue oldValue, TValue newValue)
    {
        _valueChanged?.Invoke(source, oldValue, newValue);

        ValueChanged?.Invoke(
            this,
            new DefinitionPropertyChangedEventArgs(source, oldValue, newValue));
    }
}