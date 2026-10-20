using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Controls;
/// <summary>A modal window whose completion is explicit and focus is trapped by the root.</summary>
public class Dialog : Window
{
    private TaskCompletionSource<DialogResult>? _completion;
    private Workspace? _owner;
    /// <summary>Initializes a new dialog instance.</summary>
    public Dialog() : base()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "Dialog";
    /// <summary>Gets or sets is open.</summary>
    public bool IsOpen { get; private set; }
    /// <summary>Gets or sets result.</summary>
    public DialogResult Result { get; private set; }
    /// <summary>Gets or sets default button.</summary>
    public Button? DefaultButton { get; set; }

    /// <summary>Performs show async using the current control state.</summary>
    public Task<DialogResult> ShowAsync(Workspace root) => root.ShowDialog(this);
    internal Task<DialogResult> Prepare(Workspace root)
    {
        if (IsOpen || _completion is not null)
            throw new InvalidOperationException("Dialog is already open.");
        _owner = root;
        Result = DialogResult.None;
        _completion = new TaskCompletionSource<DialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        return _completion.Task;
    }

    internal void Open()
    {
        IsOpen = true;
        OnOpening();
    }

    /// <summary>Performs on opening using the current control state.</summary>
    protected virtual void OnOpening()
    {
    }

    /// <summary>Performs complete using the current control state.</summary>
    public void Complete(DialogResult result)
    {
        if (_owner is null)
            return;
        Result = result;
        Workspace root = _owner;
        root.Mutate(() => root.CloseOverlay(this));
    }

    /// <inheritdoc/>
    public override void Close() => Complete(DialogResult.Cancel);
    internal override void NotifyClosed()
    {
        IsOpen = false;
        if (Result == DialogResult.None)
            Result = DialogResult.Cancel;
        var completion = _completion;
        _completion = null;
        _owner = null;
        base.NotifyClosed();
        completion?.TrySetResult(Result);
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        if (args.Input.Type == ControlInputEventType.KeyDown && args.Input.Key == Keys.Escape)
        {
            Close();
            args.Handled = true;
            return;
        }

        if (args.Input.Type == ControlInputEventType.KeyDown && args.Input.Key == Keys.Enter && DefaultButton is Button button && button.IsAvailable())
        {
            OnAccept();
            args.Handled = true;
            return;
        }

        base.OnInput(context, args);
    }

    /// <summary>Performs on accept using the current control state.</summary>
    protected virtual void OnAccept()
    {
        if (DefaultButton is Button button)
            button.PerformClick();
        else
            Complete(DialogResult.OK);
    }

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
