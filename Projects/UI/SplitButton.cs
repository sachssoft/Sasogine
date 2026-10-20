namespace Sachssoft.Engine.Controls;
/// <summary>Combines an independent primary command button with a separate popup arrow.</summary>
public class SplitButton : ContainerControl
{
    private readonly Button _primary = new();
    private readonly DropDownButton _arrow = new()
    {
        Width = 28,
        MinWidth = 0,
        Padding = Insets2.Zero
    };
    /// <summary>Creates the primary and secondary buttons.</summary>
    public SplitButton()
    {
        base.Add(_primary);
        base.Add(_arrow);
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "SplitButton";
    /// <summary>Gets the main button, including deterministic IsPressed state and Clicked.</summary>
    public Button PrimaryButton => _primary;
    /// <summary>Gets the popup arrow; its availability is independent of the primary command.</summary>
    public DropDownButton DropDown => _arrow;

    /// <summary>Gets or sets arbitrary primary content.</summary>
    public Control? Content
    {
        get => _primary.Content;
        set
        {
            if (ReferenceEquals(_primary.Content, value))
                return;
            _primary.Content = value;
            NotifyPropertyChanged(ContentArgs);
        }
    }

    /// <summary>Gets or sets the primary command.</summary>
    public ICommand? Command
    {
        get => _primary.Command;
        set
        {
            if (ReferenceEquals(_primary.Command, value))
                return;
            _primary.Command = value;
            NotifyPropertyChanged(CommandArgs);
        }
    }

    /// <summary>Gets or sets the primary command parameter.</summary>
    public object? CommandParameter
    {
        get => _primary.CommandParameter;
        set
        {
            if (Equals(_primary.CommandParameter, value))
                return;
            _primary.CommandParameter = value;
            NotifyPropertyChanged(ParameterArgs);
        }
    }

    /// <summary>Gets or sets the dropdown menu.</summary>
    public ContextMenu? Menu
    {
        get => _arrow.Menu;
        set
        {
            if (ReferenceEquals(_arrow.Menu, value))
                return;
            _arrow.Menu = value;
            NotifyPropertyChanged(MenuArgs);
        }
    }

    private static readonly System.ComponentModel.PropertyChangedEventArgs ContentArgs = new(nameof(Content)), CommandArgs = new(nameof(Command)), ParameterArgs = new(nameof(CommandParameter)), MenuArgs = new(nameof(Menu));
    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        _primary.Measure(context, available);
        _arrow.Measure(context, available);
        return new Size2(_primary.DesiredSize.Width + 28, MathF.Max(_primary.DesiredSize.Height, _arrow.DesiredSize.Height));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 b)
    {
        float w = MathF.Min(28, b.Width);
        _primary.Arrange(new(b.X, b.Y, b.Width - w, b.Height), true);
        _arrow.Arrange(new(b.X + b.Width - w, b.Y, w, b.Height), true);
    }
}
