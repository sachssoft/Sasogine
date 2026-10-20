using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Globalization;

namespace Sachssoft.Engine.Controls;
/// <summary>Validated date input with a calendar popup owned by the root.</summary>
public class DatePicker : ContainerControl
{
    private readonly TextBox _editor = new()
    {
        StyleId = "DatePicker.Editor"
    };
    private readonly Button _drop = new()
    {
        Padding = Insets2.Zero
    };
    private Calendar? _popup;
    private ChangeTracker<DateOnly?> _date;
    private ChangeTracker<string> _format, _culture;
    private bool _syncing, _dirty, _wasFocused;
    /// <summary>Creates a date picker.</summary>
    public DatePicker() : base()
    {
        _drop.Content = new ControlGlyph(ControlGlyphKind.Calendar);
        base.Add(_editor);
        base.Add(_drop);
        _drop.Clicked += (_, _) =>
        {
            if (IsDropDownOpen)
                CloseDropDown();
            else
                OpenDropDown();
        };
        _editor.Submitted += (_, _) => CommitEdit();
        _editor.TextChanged += (_, _) =>
        {
            if (!_syncing)
            {
                _dirty = true;
                _editor.HasError = !TryRead(out _);
            }
        };
    }

    /// <summary>Gets or sets the selected date, or null.</summary>
    public DateOnly? SelectedDate
    {
        get => ConfigSelectedDate;
        set
        {
            if (value is DateOnly d && (d < ConfigMinimumDate || d > ConfigMaximumDate))
                throw new ArgumentOutOfRangeException(nameof(value));
            ConfigSelectedDate = value;
            _dirty = false;
            Notify();
            Sync();
        }
    }

    /// <summary>Gets the editor.</summary>
    public TextBox Editor => _editor;
    /// <summary>Gets the open calendar.</summary>
    public Calendar? Calendar => _popup;
    /// <summary>Gets whether the calendar is open.</summary>
    public bool IsDropDownOpen => _popup is not null;

    /// <summary>Occurs after the date changes.</summary>
    public event EventHandler? SelectionChanged;
    /// <inheritdoc/>
    protected override string DefaultStyleName => "DatePicker";
    private CultureInfo Culture => ConfigCultureName.Length == 0 ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(ConfigCultureName);

    private void Notify()
    {
        bool ready = _date.WasEnsured;
        bool changed = _date.EnsureChange(SelectedDate);
        if (ready && changed)
            SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private bool TryRead(out DateOnly? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(_editor.Text))
            return true;
        if (!DateOnly.TryParseExact(_editor.Text, ConfigDateFormat, Culture, DateTimeStyles.None, out DateOnly date) || date < ConfigMinimumDate || date > ConfigMaximumDate)
            return false;
        value = date;
        return true;
    }

    private void Sync()
    {
        if (_dirty)
            return;
        _syncing = true;
        try
        {
            _editor.Text = SelectedDate?.ToString(ConfigDateFormat, Culture) ?? "";
            _editor.Placeholder = ConfigDateFormat;
            _editor.HasError = false;
        }
        finally
        {
            _syncing = false;
        }
    }

    /// <summary>Commits valid text; invalid input leaves the selected date unchanged.</summary>
    public bool CommitEdit()
    {
        if (!TryRead(out DateOnly? value))
        {
            _editor.HasError = true;
            return false;
        }

        SelectedDate = value;
        return true;
    }

    /// <summary>Opens a calendar without borrowing content from another tree.</summary>
    public void OpenDropDown()
    {
        if (Workspace is null || IsDropDownOpen || !IsAvailable() || ConfigIsReadOnly)
            return;
        var d = new Calendar
        {
            SelectedDate = SelectedDate,
            DisplayMonth = SelectedDate ?? ConfigDisplayMonth,
            MinimumDate = ConfigMinimumDate,
            MaximumDate = ConfigMaximumDate,
            Today = ConfigToday,
            FirstDayOfWeek = ConfigFirstDayOfWeek,
            CultureName = ConfigCultureName,
            Width = 310,
            Height = 292
        };
        var calendar = d;
        _popup = calendar;
        calendar.SelectionChanged += (_, _) =>
        {
            SelectedDate = calendar.SelectedDate;
            CloseDropDown();
        };
        float h = MathF.Min(292, Workspace.Size.Height), y = Bounds.Bottom + h <= Workspace.Size.Height ? Bounds.Bottom : MathF.Max(0, Bounds.Y - h);
        Workspace.ShowPopup(calendar, new Bounds2(Bounds.X, y, 310, h), this, () =>
        {
            ConfigDisplayMonth = calendar.DisplayMonth;
            _popup = null;
        });
    }

    /// <summary>Closes the calendar and restores focus through the root.</summary>
    public void CloseDropDown()
    {
        if (_popup is Calendar popup)
            Workspace?.ClosePopup(popup);
    }

    internal override void Prepare()
    {
        base.Prepare();
        if (ConfigMinimumDate > ConfigMaximumDate || SelectedDate is DateOnly d && (d < ConfigMinimumDate || d > ConfigMaximumDate))
            throw new InvalidOperationException("Invalid date picker bounds.");
        _format.EnsureChange(ConfigDateFormat);
        _culture.EnsureChange(ConfigCultureName);
        Notify();
        Sync();
        _editor.IsReadOnly = ConfigIsReadOnly;
        _drop.IsEnabled = !ConfigIsReadOnly;
        if (!IsAvailable() || ConfigIsReadOnly)
            CloseDropDown();
        if (_popup is Calendar calendar)
        {
            calendar.MinimumDate = ConfigMinimumDate;
            calendar.MaximumDate = ConfigMaximumDate;
            calendar.FirstDayOfWeek = ConfigFirstDayOfWeek;
            calendar.CultureName = ConfigCultureName;
            calendar.Today = ConfigToday;
        }
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        _editor.Measure(context, available);
        _drop.Measure(context, available);
        return new Size2(MathF.Min(190, available.Width), MathF.Min(36, available.Height));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 b)
    {
        float width = MathF.Min(32, b.Width);
        _editor.Arrange(new Bounds2(b.X, b.Y, MathF.Max(0, b.Width - width), b.Height), true);
        _drop.Arrange(new Bounds2(b.Right - width, b.Y, width, b.Height), true);
    }

    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        if (_wasFocused && !_editor.IsFocused && _dirty)
            CommitEdit();
        _wasFocused = _editor.IsFocused;
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var e = args.Input;
        if (e.Type == ControlInputEventType.KeyDown && (e.Key == Keys.F4 || e.Alt && e.Key == Keys.Down))
        {
            OpenDropDown();
            args.Handled = true;
        }
        else if (e.Type == ControlInputEventType.KeyDown && e.Key == Keys.Escape)
        {
            _dirty = false;
            Sync();
            CloseDropDown();
            args.Handled = true;
        }
    }

    private StyleProperty<bool> _styleIsReadOnly = new(false);
    private static readonly System.ComponentModel.PropertyChangedEventArgs IsReadOnlyChangedArgs = new(nameof(IsReadOnly));
    internal bool ConfigIsReadOnly
    {
        get => _styleIsReadOnly.Value;
        set
        {
            if (_styleIsReadOnly.SetLocal(value))
                NotifyPropertyChanged(IsReadOnlyChangedArgs);
        }
    }

    /// <summary>Gets or sets IsReadOnly; a local assignment overrides the style.</summary>
    public bool IsReadOnly { get => ConfigIsReadOnly; set => ConfigIsReadOnly = value; }

    private StyleProperty<string> _styleCultureName = new(string.Empty);
    private static readonly System.ComponentModel.PropertyChangedEventArgs CultureNameChangedArgs = new(nameof(CultureName));
    internal string ConfigCultureName
    {
        get => _styleCultureName.Value;
        set
        {
            if (_styleCultureName.SetLocal(value))
                NotifyPropertyChanged(CultureNameChangedArgs);
        }
    }

    /// <summary>Gets or sets CultureName; a local assignment overrides the style.</summary>
    public string CultureName { get => ConfigCultureName; set => ConfigCultureName = value; }

    private StyleProperty<DateOnly?> _styleSelectedDate = new(null);
    private static readonly System.ComponentModel.PropertyChangedEventArgs SelectedDateChangedArgs = new(nameof(SelectedDate));
    internal DateOnly? ConfigSelectedDate
    {
        get => _styleSelectedDate.Value;
        set
        {
            if (_styleSelectedDate.SetLocal(value))
                NotifyPropertyChanged(SelectedDateChangedArgs);
        }
    }

    private StyleProperty<DateOnly> _styleDisplayMonth = new(new DateOnly(2000, 1, 1));
    private static readonly System.ComponentModel.PropertyChangedEventArgs DisplayMonthChangedArgs = new(nameof(DisplayMonth));
    internal DateOnly ConfigDisplayMonth
    {
        get => _styleDisplayMonth.Value;
        set
        {
            if (_styleDisplayMonth.SetLocal(value))
                NotifyPropertyChanged(DisplayMonthChangedArgs);
        }
    }

    /// <summary>Gets or sets DisplayMonth; a local assignment overrides the style.</summary>
    public DateOnly DisplayMonth { get => ConfigDisplayMonth; set => ConfigDisplayMonth = value; }

    private StyleProperty<DateOnly> _styleMinimumDate = new(DateOnly.MinValue);
    private static readonly System.ComponentModel.PropertyChangedEventArgs MinimumDateChangedArgs = new(nameof(MinimumDate));
    internal DateOnly ConfigMinimumDate
    {
        get => _styleMinimumDate.Value;
        set
        {
            if (_styleMinimumDate.SetLocal(value))
                NotifyPropertyChanged(MinimumDateChangedArgs);
        }
    }

    /// <summary>Gets or sets MinimumDate; a local assignment overrides the style.</summary>
    public DateOnly MinimumDate { get => ConfigMinimumDate; set => ConfigMinimumDate = value; }

    private StyleProperty<DateOnly> _styleMaximumDate = new(DateOnly.MaxValue);
    private static readonly System.ComponentModel.PropertyChangedEventArgs MaximumDateChangedArgs = new(nameof(MaximumDate));
    internal DateOnly ConfigMaximumDate
    {
        get => _styleMaximumDate.Value;
        set
        {
            if (_styleMaximumDate.SetLocal(value))
                NotifyPropertyChanged(MaximumDateChangedArgs);
        }
    }

    /// <summary>Gets or sets MaximumDate; a local assignment overrides the style.</summary>
    public DateOnly MaximumDate { get => ConfigMaximumDate; set => ConfigMaximumDate = value; }

    private StyleProperty<DateOnly?> _styleToday = new(null);
    private static readonly System.ComponentModel.PropertyChangedEventArgs TodayChangedArgs = new(nameof(Today));
    internal DateOnly? ConfigToday
    {
        get => _styleToday.Value;
        set
        {
            if (_styleToday.SetLocal(value))
                NotifyPropertyChanged(TodayChangedArgs);
        }
    }

    /// <summary>Gets or sets Today; a local assignment overrides the style.</summary>
    public DateOnly? Today { get => ConfigToday; set => ConfigToday = value; }

    private StyleProperty<DayOfWeek> _styleFirstDayOfWeek = new(DayOfWeek.Monday);
    private static readonly System.ComponentModel.PropertyChangedEventArgs FirstDayOfWeekChangedArgs = new(nameof(FirstDayOfWeek));
    internal DayOfWeek ConfigFirstDayOfWeek
    {
        get => _styleFirstDayOfWeek.Value;
        set
        {
            if (_styleFirstDayOfWeek.SetLocal(value))
                NotifyPropertyChanged(FirstDayOfWeekChangedArgs);
        }
    }

    /// <summary>Gets or sets FirstDayOfWeek; a local assignment overrides the style.</summary>
    public DayOfWeek FirstDayOfWeek { get => ConfigFirstDayOfWeek; set => ConfigFirstDayOfWeek = value; }

    private StyleProperty<string> _styleDateFormat = new("yyyy-MM-dd");
    private static readonly System.ComponentModel.PropertyChangedEventArgs DateFormatChangedArgs = new(nameof(DateFormat));
    internal string ConfigDateFormat
    {
        get => _styleDateFormat.Value;
        set
        {
            if (_styleDateFormat.SetLocal(value))
                NotifyPropertyChanged(DateFormatChangedArgs);
        }
    }

    /// <summary>Gets or sets DateFormat; a local assignment overrides the style.</summary>
    public string DateFormat { get => ConfigDateFormat; set => ConfigDateFormat = value; }

    /// <summary>Applies this control's explicit style values without replacing local assignments.</summary>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "IsReadOnly":
                    if (_styleIsReadOnly.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(IsReadOnlyChangedArgs);
                    break;
                case "CultureName":
                    if (_styleCultureName.Apply(Converters.Convert<string>("String", entry.Value)))
                        NotifyPropertyChanged(CultureNameChangedArgs);
                    break;
                case "SelectedDate":
                    if (_styleSelectedDate.Apply(Converters.Convert<DateOnly?>("NullableDate", entry.Value)))
                        NotifyPropertyChanged(SelectedDateChangedArgs);
                    break;
                case "DisplayMonth":
                    if (_styleDisplayMonth.Apply(Converters.Convert<DateOnly>("Date", entry.Value)))
                        NotifyPropertyChanged(DisplayMonthChangedArgs);
                    break;
                case "MinimumDate":
                    if (_styleMinimumDate.Apply(Converters.Convert<DateOnly>("Date", entry.Value)))
                        NotifyPropertyChanged(MinimumDateChangedArgs);
                    break;
                case "MaximumDate":
                    if (_styleMaximumDate.Apply(Converters.Convert<DateOnly>("Date", entry.Value)))
                        NotifyPropertyChanged(MaximumDateChangedArgs);
                    break;
                case "Today":
                    if (_styleToday.Apply(Converters.Convert<DateOnly?>("NullableDate", entry.Value)))
                        NotifyPropertyChanged(TodayChangedArgs);
                    break;
                case "FirstDayOfWeek":
                    if (_styleFirstDayOfWeek.Apply(Converters.Convert<DayOfWeek>("DayOfWeek", entry.Value)))
                        NotifyPropertyChanged(FirstDayOfWeekChangedArgs);
                    break;
                case "DateFormat":
                    if (_styleDateFormat.Apply(Converters.Convert<string>("String", entry.Value)))
                        NotifyPropertyChanged(DateFormatChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "IsReadOnly":
                return _styleIsReadOnly.HasLocalValue;
            case "CultureName":
                return _styleCultureName.HasLocalValue;
            case "SelectedDate":
                return _styleSelectedDate.HasLocalValue;
            case "DisplayMonth":
                return _styleDisplayMonth.HasLocalValue;
            case "MinimumDate":
                return _styleMinimumDate.HasLocalValue;
            case "MaximumDate":
                return _styleMaximumDate.HasLocalValue;
            case "Today":
                return _styleToday.HasLocalValue;
            case "FirstDayOfWeek":
                return _styleFirstDayOfWeek.HasLocalValue;
            case "DateFormat":
                return _styleDateFormat.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "IsReadOnly":
                if (_styleIsReadOnly.ClearLocal())
                    NotifyPropertyChanged(IsReadOnlyChangedArgs);
                return;
            case "CultureName":
                if (_styleCultureName.ClearLocal())
                    NotifyPropertyChanged(CultureNameChangedArgs);
                return;
            case "SelectedDate":
                if (_styleSelectedDate.ClearLocal())
                    NotifyPropertyChanged(SelectedDateChangedArgs);
                return;
            case "DisplayMonth":
                if (_styleDisplayMonth.ClearLocal())
                    NotifyPropertyChanged(DisplayMonthChangedArgs);
                return;
            case "MinimumDate":
                if (_styleMinimumDate.ClearLocal())
                    NotifyPropertyChanged(MinimumDateChangedArgs);
                return;
            case "MaximumDate":
                if (_styleMaximumDate.ClearLocal())
                    NotifyPropertyChanged(MaximumDateChangedArgs);
                return;
            case "Today":
                if (_styleToday.ClearLocal())
                    NotifyPropertyChanged(TodayChangedArgs);
                return;
            case "FirstDayOfWeek":
                if (_styleFirstDayOfWeek.ClearLocal())
                    NotifyPropertyChanged(FirstDayOfWeekChangedArgs);
                return;
            case "DateFormat":
                if (_styleDateFormat.ClearLocal())
                    NotifyPropertyChanged(DateFormatChangedArgs);
                return;
            default:
                base.ResetStyleProperty(property);
                return;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
        if (_styleIsReadOnly.BeginStyle(false))
            NotifyPropertyChanged(IsReadOnlyChangedArgs);
        if (_styleCultureName.BeginStyle(string.Empty))
            NotifyPropertyChanged(CultureNameChangedArgs);
        if (_styleSelectedDate.BeginStyle(null))
            NotifyPropertyChanged(SelectedDateChangedArgs);
        if (_styleDisplayMonth.BeginStyle(new DateOnly(2000, 1, 1)))
            NotifyPropertyChanged(DisplayMonthChangedArgs);
        if (_styleMinimumDate.BeginStyle(DateOnly.MinValue))
            NotifyPropertyChanged(MinimumDateChangedArgs);
        if (_styleMaximumDate.BeginStyle(DateOnly.MaxValue))
            NotifyPropertyChanged(MaximumDateChangedArgs);
        if (_styleToday.BeginStyle(null))
            NotifyPropertyChanged(TodayChangedArgs);
        if (_styleFirstDayOfWeek.BeginStyle(DayOfWeek.Monday))
            NotifyPropertyChanged(FirstDayOfWeekChangedArgs);
        if (_styleDateFormat.BeginStyle("yyyy-MM-dd"))
            NotifyPropertyChanged(DateFormatChangedArgs);
    }

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleIsReadOnly.CommitStyle())
            NotifyPropertyChanged(IsReadOnlyChangedArgs);
        if (_styleCultureName.CommitStyle())
            NotifyPropertyChanged(CultureNameChangedArgs);
        if (_styleSelectedDate.CommitStyle())
            NotifyPropertyChanged(SelectedDateChangedArgs);
        if (_styleDisplayMonth.CommitStyle())
            NotifyPropertyChanged(DisplayMonthChangedArgs);
        if (_styleMinimumDate.CommitStyle())
            NotifyPropertyChanged(MinimumDateChangedArgs);
        if (_styleMaximumDate.CommitStyle())
            NotifyPropertyChanged(MaximumDateChangedArgs);
        if (_styleToday.CommitStyle())
            NotifyPropertyChanged(TodayChangedArgs);
        if (_styleFirstDayOfWeek.CommitStyle())
            NotifyPropertyChanged(FirstDayOfWeekChangedArgs);
        if (_styleDateFormat.CommitStyle())
            NotifyPropertyChanged(DateFormatChangedArgs);
    }
}
