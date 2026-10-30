# Numeric formatting

`ValueBar`, `ValueTextBox`, `NumericSpinner`, and `RotationControl` expose `NumberFormat` and `CultureName`. Both properties support central styles, local overrides, and property change notifications. The default culture is invariant; use `"de-DE"` for German number separators.

| NumberFormat | Example for 12.5, invariant culture |
| --- | --- |
| `"0"` | `13` |
| `"0.00"` | `12.50` |
| `"0.##"` | `12.5` |
| `"N2"` | `12.50` with grouping for larger values |
| `"0.0 'km/h'"` | `12.5 km/h` |

Formatting changes the display, not the stored value. `NumericSpinner.DecimalPlaces` sets the stored precision from 0 to 28; `ValueTextBox.Step` and `ValueBar.Step` control their value increments. To edit integers, use `DecimalPlaces = 0` or a whole-number range with `Step = 1`.

```csharp
var integer = new NumericSpinner
{
    DecimalPlaces = 0,
    Increment = 1,
    NumberFormat = "0"
};

var decimalValue = new NumericSpinner
{
    DecimalPlaces = 2,
    Increment = 0.01m,
    NumberFormat = "0.00",
    CultureName = "de-DE"
};

var speed = new ValueTextBox
{
    Minimum = 0,
    Maximum = 150,
    Step = 0.1f,
    NumberFormat = "0.0 'km/h'"
};

var style = new Style
{
    NumberFormat = "0.##",
    CultureName = "de-DE"
};
```

`NumericSpinner` uses its existing fixed `DecimalPlaces` display when `NumberFormat` is empty. `ValueBar` and `ValueTextBox` default to `"0.###"`; `RotationControl` defaults to `"0.#°"`.

While editing, numeric text fields show the plain value with enough precision to preserve it. Units, grouping, percentages, and display-only rounding are removed from this editable representation. Input uses the configured culture; invalid input preserves the committed value. Standard percentage formats such as `"P0"` multiply the displayed value by 100, while editing still uses the underlying value, for example `0.25` for `25 %`.

`Spinner` is a loading animation with no numeric text. Range-only controls do not gain a text label through this feature. `IconValueDisplay` already supports `ValueFormat` and `ValueFormatProvider`; inspector text editors accept explicit formatting and parsing delegates.
