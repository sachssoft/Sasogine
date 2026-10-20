using Microsoft.Xna.Framework;
using System;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Controls;
/// <summary>A modal message with explicit result buttons and a semantic atlas icon.</summary>
public sealed class MessageBox : Dialog
{
    private readonly Label _message;
    private readonly WrapPanel _buttons;
    /// <summary>Initializes a new message box instance.</summary>
    public MessageBox() : base()
    {
        _message = new Label()
        {
            WrapText = true,
            HorizontalAlignment = ControlAlignment.Stretch
        };
        _buttons = new WrapPanel()
        {
            Spacing = 8
        };
        Content = new MessageBoxLayout(this, _message, _buttons);
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "MessageBox";

    /// <inheritdoc/>
    protected override void OnOpening()
    {
        _message.Text = ConfigMessage;
        _buttons.Clear();
        DefaultButton = null;
        void Add(string text, DialogResult result)
        {
            var button = new Button()
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
    protected override void OnAccept() => Complete(ConfigButtons is MessageBoxButtons.YesNo or MessageBoxButtons.YesNoCancel ? DialogResult.Yes : DialogResult.OK);
    /// <summary>Performs show using the current control state.</summary>
    public static Task<DialogResult> Show(Workspace root, string message, string title = "", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information) => new MessageBox()
    {
        Title = title,
        Message = message,
        Buttons = buttons,
        Icon = icon
    }.ShowAsync(root);
    private string _storedMessage = string.Empty;
    private static readonly System.ComponentModel.PropertyChangedEventArgs MessageStorageArgs = new("Message");
    internal string ConfigMessage { get => _storedMessage; set => SetNotify(ref _storedMessage, value, MessageStorageArgs); }
    /// <summary>Gets or sets the value for Message.</summary>
    public string Message { get => ConfigMessage; set => ConfigMessage = value; }

    private MessageBoxButtons _storedButtons = MessageBoxButtons.OK;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ButtonsStorageArgs = new("Buttons");
    internal MessageBoxButtons ConfigButtons { get => _storedButtons; set => SetNotify(ref _storedButtons, value, ButtonsStorageArgs); }
    /// <summary>Gets or sets the value for Buttons.</summary>
    public MessageBoxButtons Buttons { get => ConfigButtons; set => ConfigButtons = value; }

    private MessageBoxIcon _storedIcon = MessageBoxIcon.Information;
    private static readonly System.ComponentModel.PropertyChangedEventArgs IconStorageArgs = new("Icon");
    internal MessageBoxIcon ConfigIcon { get => _storedIcon; set => SetNotify(ref _storedIcon, value, IconStorageArgs); }
    /// <summary>Gets or sets the value for Icon.</summary>
    public MessageBoxIcon Icon { get => ConfigIcon; set => ConfigIcon = value; }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            default:
                base.ResetStyleProperty(property);
                return;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
    }
}
