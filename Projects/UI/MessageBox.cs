
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Controls;

/// <summary>
/// Represents a modal dialog that displays a message, a semantic icon,
/// and a configurable set of response buttons.
/// </summary>
/// <remarks>
/// A message box can be displayed asynchronously using <see cref="ShowAsync"/>
/// or through a completion callback using <see cref="Show"/>.
/// </remarks>
public sealed class MessageBox : Dialog
{
    private readonly Label _message;
    private readonly WrapPanel _buttons;

    private string _storedMessage = string.Empty;
    private MessageBoxButtons _storedButtons = MessageBoxButtons.OK;
    private MessageBoxIcon _storedIcon = MessageBoxIcon.Information;

    private static readonly PropertyChangedEventArgs MessageStorageArgs = new(nameof(Message));
    private static readonly PropertyChangedEventArgs ButtonsStorageArgs = new(nameof(Buttons));
    private static readonly PropertyChangedEventArgs IconStorageArgs = new(nameof(Icon));

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBox"/> class.
    /// </summary>
    public MessageBox()
    {
        _message = new Label
        {
            WrapText = true,
            HorizontalAlignment = ControlAlignment.Stretch
        };

        _buttons = new WrapPanel
        {
            Spacing = 8
        };

        Content = new MessageBoxLayout(this, _message, _buttons);
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "MessageBox";

    /// <summary>
    /// Gets or sets the message displayed in the dialog.
    /// </summary>
    public string Message
    {
        get => ConfigMessage;
        set => ConfigMessage = value;
    }

    /// <summary>
    /// Gets or sets the combination of response buttons displayed in the dialog.
    /// </summary>
    public MessageBoxButtons Buttons
    {
        get => ConfigButtons;
        set => ConfigButtons = value;
    }

    /// <summary>
    /// Gets or sets the semantic icon displayed alongside the message.
    /// </summary>
    public MessageBoxIcon Icon
    {
        get => ConfigIcon;
        set => ConfigIcon = value;
    }

    /// <summary>
    /// Gets or sets the message value used by the dialog.
    /// </summary>
    internal string ConfigMessage
    {
        get => _storedMessage;
        set => SetNotify(ref _storedMessage, value, MessageStorageArgs);
    }

    /// <summary>
    /// Gets or sets the button configuration used by the dialog.
    /// </summary>
    internal MessageBoxButtons ConfigButtons
    {
        get => _storedButtons;
        set => SetNotify(ref _storedButtons, value, ButtonsStorageArgs);
    }

    /// <summary>
    /// Gets or sets the semantic icon configuration used by the dialog.
    /// </summary>
    internal MessageBoxIcon ConfigIcon
    {
        get => _storedIcon;
        set => SetNotify(ref _storedIcon, value, IconStorageArgs);
    }

    /// <summary>
    /// Displays a message box asynchronously and returns its result when closed.
    /// </summary>
    public static Task<DialogResult> ShowAsync(
        Workspace root,
        string message,
        string title = "",
        MessageBoxButtons buttons = MessageBoxButtons.OK,
        MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return Create(message, title, buttons, icon).ShowAsync(root);
    }

    /// <summary>
    /// Displays a message box and invokes a callback when it closes.
    /// </summary>
    public static void Show(
        Workspace root,
        string message,
        Action<DialogResult> completed,
        string title = "",
        MessageBoxButtons buttons = MessageBoxButtons.OK,
        MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        ArgumentNullException.ThrowIfNull(completed);

        Create(message, title, buttons, icon).Show(root, completed);
    }

    /// <summary>
    /// Creates a message box with the specified configuration.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="title">The title displayed in the dialog header.</param>
    /// <param name="buttons">The combination of response buttons to display.</param>
    /// <param name="icon">The semantic icon displayed alongside the message.</param>
    /// <returns>A configured message box instance.</returns>
    private static MessageBox Create(
        string message,
        string title,
        MessageBoxButtons buttons,
        MessageBoxIcon icon)
    {
        return new MessageBox
        {
            Title = title,
            Message = message,
            Buttons = buttons,
            Icon = icon
        };
    }

    /// <inheritdoc/>
    protected override void OnOpening()
    {
        base.OnOpening();

        _message.Text = ConfigMessage;
        _buttons.Clear();
        DefaultButton = null;

        void Add(string text, DialogResult result)
        {
            var button = new Button
            {
                Content = new TextBlock
                {
                    Text = text
                },
                Width = 88,
                Height = 32
            };

            button.Clicked += (_, _) => Complete(result);

            _buttons.Add(button);
            DefaultButton ??= button;
        }

        switch (ConfigButtons)
        {
            case MessageBoxButtons.OK:
                Add("OK", DialogResult.OK);
                break;

            case MessageBoxButtons.OKCancel:
                Add("OK", DialogResult.OK);
                Add("Cancel", DialogResult.Cancel);
                break;

            case MessageBoxButtons.YesNo:
                Add("Yes", DialogResult.Yes);
                Add("No", DialogResult.No);
                break;

            case MessageBoxButtons.YesNoCancel:
                Add("Yes", DialogResult.Yes);
                Add("No", DialogResult.No);
                Add("Cancel", DialogResult.Cancel);
                break;

            default:
                throw new InvalidOperationException("Invalid message box buttons.");
        }
    }

    /// <inheritdoc/>
    protected override void OnAccept()
    {
        Complete(ConfigButtons is MessageBoxButtons.YesNo or MessageBoxButtons.YesNoCancel
            ? DialogResult.Yes
            : DialogResult.OK);
    }

    /// <inheritdoc/>
    internal override bool IsStyleLocal(string property)
    {
        return base.IsStyleLocal(property);
    }

    /// <inheritdoc/>
    public override void ResetStyleProperty(string property)
    {
        base.ResetStyleProperty(property);
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
    }
}
