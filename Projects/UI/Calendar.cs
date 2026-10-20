using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Sachssoft.Engine.Controls;
/// <summary>Six-week calendar with bounded date selection, culture, keyboard navigation and explicit today.</summary>
public class Calendar : ContainerControl
{
    private readonly RepeatButton _previous = new()
    {
        Padding = Insets2.Zero
    }, _next = new()
    {
        Padding = Insets2.Zero
    };
    private readonly Label _caption = new()
    {
        TextHorizontalAlignment = ControlAlignment.Center
    };
    private readonly Label[] _weekdays = new Label[7];
    private readonly Button[] _days = new Button[42];
    private readonly TextBlock[] _dayCaptions = new TextBlock[42];
    private readonly DateOnly? [] _dates = new DateOnly? [42];
    private readonly ReadOnlyCollection<DateOnly?> _dateView;
    private readonly ReadOnlyCollection<Button> _dayView;
    private ChangeTracker<DateOnly?> _selected;
    private ChangeTracker<DateOnly> _month;
    /// <summary>Creates a calendar. Defaults are fixed; the host supplies Today when desired.</summary>
    public Calendar() : base()
    {
        _previous.Content = new ControlGlyph(ControlGlyphKind.Left);
        _next.Content = new ControlGlyph(ControlGlyphKind.Right);
        _dateView = Array.AsReadOnly(_dates);
        _dayView = Array.AsReadOnly(_days);
        base.Add(_previous);
        base.Add(_caption);
        base.Add(_next);
        _previous.Clicked += (_, _) => MoveMonth(-1);
        _next.Clicked += (_, _) => MoveMonth(1);
        for (int i = 0; i < 7; i++)
        {
            _weekdays[i] = new Label()
            {
                TextHorizontalAlignment = ControlAlignment.Center
            };
            base.Add(_weekdays[i]);
        }

        for (int i = 0; i < 42; i++)
        {
            int index = i;
            var caption = new TextBlock();
            _dayCaptions[i] = caption;
            var button = new Button()
            {
                Padding = Insets2.Zero,
                StyleId = "Calendar.Day",
                Content = caption
            };
            button.Clicked += (_, _) =>
            {
                if (_dates[index] is DateOnly date)
                    SelectedDate = date;
            };
            _days[i] = button;
            base.Add(button);
        }
    }

    /// <summary>Gets or sets the selected date within configured bounds.</summary>
    public DateOnly? SelectedDate
    {
        get => ConfigSelectedDate;
        set
        {
            if (value is DateOnly d && (d < ConfigMinimumDate || d > ConfigMaximumDate))
                throw new ArgumentOutOfRangeException(nameof(value));
            ConfigSelectedDate = value;
            if (value is DateOnly date)
                DisplayMonth = date;
            Notify();
        }
    }

    /// <summary>Gets or sets the month to display.</summary>
    public DateOnly DisplayMonth { get => ConfigDisplayMonth; set => ConfigDisplayMonth = new DateOnly(value.Year, value.Month, 1); }
    /// <summary>Gets dates in the visible six-week grid; null represents a date outside DateOnly limits.</summary>
    public IReadOnlyList<DateOnly?> VisibleDates => _dateView;
    /// <summary>Gets the day buttons.</summary>
    public IReadOnlyList<Button> DayButtons => _dayView;

    /// <summary>Occurs after date selection changes.</summary>
    public event EventHandler? SelectionChanged;
    /// <summary>Occurs after month navigation.</summary>
    public event EventHandler? DisplayMonthChanged;
    /// <inheritdoc/>
    protected override string DefaultStyleName => "Calendar";
    private CultureInfo Culture => ConfigCultureName.Length == 0 ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(ConfigCultureName);

    private void Notify()
    {
        bool ready = _selected.WasEnsured;
        bool changed = _selected.EnsureChange(ConfigSelectedDate);
        if (ready && changed)
            SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private DateOnly? AdjacentMonth(int offset)
    {
        try
        {
            return new DateOnly(DisplayMonth.Year, DisplayMonth.Month, 1).AddMonths(offset);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    /// <summary>Navigates a month if it intersects the permitted range.</summary>
    public void MoveMonth(int offset)
    {
        if (AdjacentMonth(offset)is DateOnly next && MonthAllowed(next))
            DisplayMonth = next;
    }

    private bool MonthAllowed(DateOnly first) => first <= ConfigMaximumDate && new DateOnly(first.Year, first.Month, DateTime.DaysInMonth(first.Year, first.Month)) >= ConfigMinimumDate;
    internal override void Prepare()
    {
        base.Prepare();
        if (ConfigMinimumDate > ConfigMaximumDate || (int)ConfigFirstDayOfWeek < 0 || (int)ConfigFirstDayOfWeek > 6)
            throw new InvalidOperationException("Invalid calendar bounds or first weekday.");
        if (ConfigSelectedDate is DateOnly selected && (selected < ConfigMinimumDate || selected > ConfigMaximumDate))
            throw new InvalidOperationException("Selected date is outside calendar bounds.");
        var first = new DateOnly(DisplayMonth.Year, DisplayMonth.Month, 1);
        ConfigDisplayMonth = first;
        bool ready = _month.WasEnsured;
        bool changed = _month.EnsureChange(first);
        if (ready && changed)
            DisplayMonthChanged?.Invoke(this, EventArgs.Empty);
        Notify();
        var culture = Culture;
        _caption.Text = first.ToDateTime(TimeOnly.MinValue).ToString("Y", culture);
        for (int i = 0; i < 7; i++)
            _weekdays[i].Text = culture.DateTimeFormat.AbbreviatedDayNames[((int)ConfigFirstDayOfWeek + i) % 7];
        int skip = ((int)first.DayOfWeek - (int)ConfigFirstDayOfWeek + 7) % 7;
        for (int i = 0; i < 42; i++)
        {
            long number = (long)first.DayNumber - skip + i;
            DateOnly? date = number >= 0 && number <= DateOnly.MaxValue.DayNumber ? DateOnly.FromDayNumber((int)number) : null;
            _dates[i] = date;
            var button = _days[i];
            _dayCaptions[i].Text = date?.Day.ToString(culture) ?? "";
            _dayCaptions[i].StyleId = date is DateOnly day && day.Month != first.Month ? "Calendar.OutsideDayText" : "Calendar.DayText";
            button.IsEnabled = date is DateOnly d && d >= ConfigMinimumDate && d <= ConfigMaximumDate;
            button.StyleId = date == SelectedDate && date is not null ? "Calendar.SelectedDay" : date == ConfigToday && date is not null ? "Calendar.Today" : date is DateOnly other && other.Month != first.Month ? "Calendar.OutsideDay" : "Calendar.Day";
        }

        _previous.IsEnabled = AdjacentMonth(-1)is DateOnly p && MonthAllowed(p);
        _next.IsEnabled = AdjacentMonth(1)is DateOnly n && MonthAllowed(n);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        foreach (var child in Children)
            child.Measure(context, available);
        return new Size2(MathF.Min(294, available.Width), MathF.Min(272, available.Height));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 b)
    {
        float header = MathF.Min(36, b.Height), weekday = MathF.Min(26, MathF.Max(0, b.Height - header)), nav = MathF.Min(32, b.Width / 2);
        _previous.Arrange(new Bounds2(b.X, b.Y, nav, header), true);
        _next.Arrange(new Bounds2(b.Right - nav, b.Y, nav, header), true);
        _caption.Arrange(new Bounds2(b.X + nav, b.Y, MathF.Max(0, b.Width - nav * 2), header), true);
        float w = b.Width / 7, h = MathF.Max(0, b.Height - header - weekday) / 6;
        for (int i = 0; i < 7; i++)
            _weekdays[i].Arrange(new Bounds2(b.X + i * w, b.Y + header, w, weekday), true);
        for (int i = 0; i < 42; i++)
            _days[i].Arrange(new Bounds2(b.X + i % 7 * w, b.Y + header + weekday + i / 7 * h, w, h), true);
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var e = args.Input;
        if (e.Type != ControlInputEventType.KeyDown)
            return;
        if (e.Key is Keys.PageUp or Keys.PageDown)
        {
            MoveMonth(e.Key == Keys.PageUp ? -1 : 1);
            args.Handled = true;
            return;
        }

        DateOnly origin = SelectedDate ?? new DateOnly(DisplayMonth.Year, DisplayMonth.Month, 1);
        DateOnly? next = null;
        try
        {
            next = e.Key switch
            {
                Keys.Left => origin.AddDays(-1),
                Keys.Right => origin.AddDays(1),
                Keys.Up => origin.AddDays(-7),
                Keys.Down => origin.AddDays(7),
                Keys.Home => new DateOnly(origin.Year, origin.Month, 1),
                Keys.End => new DateOnly(origin.Year, origin.Month, DateTime.DaysInMonth(origin.Year, origin.Month)),
                _ => null
            };
        }
        catch (ArgumentOutOfRangeException)
        {
        }

        if (next is DateOnly date)
        {
            date = date < ConfigMinimumDate ? ConfigMinimumDate : date > ConfigMaximumDate ? ConfigMaximumDate : date;
            SelectedDate = date;
            args.Handled = true;
        }
    }

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

    /// <summary>Applies this control's explicit style values without replacing local assignments.</summary>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
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
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
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
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
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
            default:
                base.ResetStyleProperty(property);
                return;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
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
    }

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
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
    }
}
