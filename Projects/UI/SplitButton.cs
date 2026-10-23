
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Sachssoft.Engine.Controls;

/// <summary>
/// Represents a split button consisting of an independent primary
/// command button and a secondary dropdown button.
/// </summary>
/// <remarks>
/// <para>
/// The primary button executes its command after raising its
/// Clicked event. The dropdown button opens an associated
/// context menu without executing the primary command.
/// </para>
/// <para>
/// Both buttons maintain independent enabled states.
/// A primary command that cannot execute does not disable
/// the dropdown button.
/// </para>
/// <para>
/// Content, command, command parameter, and menu properties
/// are forwarded to their respective child controls.
/// </para>
/// </remarks>
public class SplitButton : ContainerControl
{
    private const float ArrowWidth = 28f;

    private readonly Button _primary = new();
    private readonly DropDownButton _arrow = new()
    {
        Width = ArrowWidth,
        MinWidth = 0,
        Padding = Insets2.Zero
    };

    private static readonly PropertyChangedEventArgs ContentArgs = new(nameof(Content));
    private static readonly PropertyChangedEventArgs PresenterArgs = new(nameof(ContentPresenter));
    private static readonly PropertyChangedEventArgs CommandArgs = new(nameof(Command));
    private static readonly PropertyChangedEventArgs ParameterArgs = new(nameof(CommandParameter));
    private static readonly PropertyChangedEventArgs MenuArgs = new(nameof(Menu));

    /// <summary>
    /// Initializes a new instance of the <see cref="SplitButton"/> class
    /// and creates its primary and dropdown buttons.
    /// </summary>
    public SplitButton()
    {
        base.Add(_primary);
        base.Add(_arrow);

        _primary.PropertyChanged += OnPrimaryPropertyChanged;
        _arrow.PropertyChanged += OnArrowPropertyChanged;
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "SplitButton";

    /// <summary>
    /// Gets the primary button responsible for normal activation
    /// and command execution.
    /// </summary>
    /// <remarks>
    /// Provides access to the primary button's Clicked event,
    /// IsPressed state, and other inherited button properties.
    /// </remarks>
    public Button PrimaryButton => _primary;

    /// <summary>
    /// Gets the secondary button responsible for opening the dropdown menu.
    /// </summary>
    /// <remarks>
    /// The dropdown button operates independently of the primary
    /// command's CanExecute state.
    /// </remarks>
    public DropDownButton DropDown => _arrow;

    /// <summary>
    /// Gets or sets the content displayed by the primary button.
    /// </summary>
    public object? Content
    {
        get => _primary.Content;
        set => _primary.Content = value;
    }

    /// <summary>
    /// Gets the control presenting the primary button's content.
    /// </summary>
    public Control? ContentPresenter => _primary.ContentPresenter;

    /// <summary>
    /// Gets or sets the command executed by the primary button.
    /// </summary>
    /// <remarks>
    /// The command is executed after the primary button's Clicked event.
    /// Its CanExecute result affects only the primary button,
    /// not the dropdown button.
    /// </remarks>
    public ICommand? Command
    {
        get => _primary.Command;
        set => _primary.Command = value;
    }

    /// <summary>
    /// Gets or sets the parameter passed to the primary command.
    /// </summary>
    /// <remarks>
    /// The parameter is used by the primary button when evaluating
    /// CanExecute and executing the command.
    /// </remarks>
    public object? CommandParameter
    {
        get => _primary.CommandParameter;
        set => _primary.CommandParameter = value;
    }

    /// <summary>
    /// Gets or sets the context menu opened by the dropdown button.
    /// </summary>
    /// <remarks>
    /// The menu belongs to the dropdown button and is independent
    /// of the primary button's command.
    /// </remarks>
    public ContextMenu? Menu
    {
        get => _arrow.Menu;
        set => _arrow.Menu = value;
    }

    /// <summary>
    /// Forwards relevant primary button property changes
    /// to the corresponding split button properties.
    /// </summary>
    private void OnPrimaryPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(Content):
                NotifyPropertyChanged(ContentArgs);
                break;

            case nameof(ContentPresenter):
                NotifyPropertyChanged(PresenterArgs);
                break;

            case nameof(Command):
                NotifyPropertyChanged(CommandArgs);
                break;

            case nameof(CommandParameter):
                NotifyPropertyChanged(ParameterArgs);
                break;
        }
    }

    /// <summary>
    /// Forwards dropdown menu changes to the split button.
    /// </summary>
    private void OnArrowPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(DropDownButton.Menu))
            NotifyPropertyChanged(MenuArgs);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        _primary.Measure(context, available);
        _arrow.Measure(context, available);

        return new Size2(
            _primary.DesiredSize.Width + ArrowWidth,
            float.Max(_primary.DesiredSize.Height, _arrow.DesiredSize.Height));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        float arrowWidth = float.Min(ArrowWidth, bounds.Width);
        float primaryWidth = bounds.Width - arrowWidth;

        _primary.Arrange(new Bounds2(
            bounds.X, bounds.Y, primaryWidth, bounds.Height), true);

        _arrow.Arrange(new Bounds2(
            bounds.X + primaryWidth, bounds.Y, arrowWidth, bounds.Height), true);
    }
}
