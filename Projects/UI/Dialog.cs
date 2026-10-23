
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Controls;

/// <summary>
/// Represents a modal window that captures focus and produces a dialog result
/// when it is completed or closed.
/// </summary>
/// <remarks>
/// A dialog can be displayed using a completion callback or asynchronously
/// through a task. The hosting workspace manages the modal overlay and focus.
/// </remarks>
public class Dialog : Window
{
    private TaskCompletionSource<DialogResult>? _completion;
    private Action<DialogResult>? _callback;
    private Workspace? _owner;

    /// <summary>
    /// Initializes a new instance of the <see cref="Dialog"/> class.
    /// </summary>
    public Dialog()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => nameof(Dialog);

    /// <summary>
    /// Gets whether the dialog is currently open.
    /// </summary>
    public bool IsOpen { get; private set; }

    /// <summary>
    /// Gets the result of the most recent dialog interaction.
    /// </summary>
    /// <remarks>
    /// The result is reset to <see cref="DialogResult.None"/> when the dialog
    /// is prepared for display.
    /// </remarks>
    public DialogResult Result { get; private set; }

    /// <summary>
    /// Gets or sets the button activated when the user accepts the dialog.
    /// </summary>
    public Button? DefaultButton { get; set; }

    /// <summary>
    /// Displays the dialog asynchronously within the specified workspace.
    /// </summary>
    /// <param name="root">The workspace that hosts the dialog.</param>
    /// <returns>
    /// A task that completes with the dialog result after the dialog closes.
    /// </returns>
    public Task<DialogResult> ShowAsync(Workspace root)
    {
        ArgumentNullException.ThrowIfNull(root);

        return root.ShowDialog(this);
    }

    /// <summary>
    /// Displays the dialog and invokes a callback when it closes.
    /// </summary>
    /// <param name="root">The workspace that hosts the dialog.</param>
    /// <param name="completed">
    /// The callback invoked with the final dialog result.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="root"/> or
    /// <paramref name="completed"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the dialog is already open or awaiting completion.
    /// </exception>
    public void Show(Workspace root, Action<DialogResult> completed)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(completed);

        if (IsOpen || _completion is not null)
            throw new InvalidOperationException("Dialog is already open.");

        _callback = completed;

        try
        {
            root.ShowDialog(this);
        }
        catch
        {
            _callback = null;
            throw;
        }
    }

    /// <summary>
    /// Prepares the dialog for display and initializes its completion task.
    /// </summary>
    /// <param name="root">The workspace that will host the dialog.</param>
    /// <returns>A task representing the pending dialog result.</returns>
    internal Task<DialogResult> Prepare(Workspace root)
    {
        ArgumentNullException.ThrowIfNull(root);

        if (IsOpen || _completion is not null)
            throw new InvalidOperationException("Dialog is already open.");

        _owner = root;
        Result = DialogResult.None;

        _completion = new TaskCompletionSource<DialogResult>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        return _completion.Task;
    }

    /// <summary>
    /// Marks the dialog as open and invokes its opening logic.
    /// </summary>
    internal void Open()
    {
        IsOpen = true;
        OnOpening();
    }

    /// <summary>
    /// Called when the dialog opens.
    /// </summary>
    /// <remarks>
    /// Override this method to initialize dialog content, configure buttons,
    /// or update the initial interaction state.
    /// </remarks>
    protected virtual void OnOpening()
    {
    }

    /// <summary>
    /// Completes the dialog with the specified result and requests its closure.
    /// </summary>
    /// <param name="result">The result returned when the dialog closes.</param>
    /// <remarks>
    /// The result is delivered after the hosting workspace removes the dialog
    /// from its overlay collection.
    /// </remarks>
    public void Complete(DialogResult result)
    {
        if (_owner is null)
            return;

        Result = result;

        Workspace root = _owner;
        root.Mutate(() => root.CloseOverlay(this));
    }

    /// <summary>
    /// Closes the dialog with <see cref="DialogResult.Cancel"/>.
    /// </summary>
    public override void Close()
    {
        Complete(DialogResult.Cancel);
    }

    /// <summary>
    /// Finalizes the dialog state and delivers its result to pending consumers.
    /// </summary>
    internal override void NotifyClosed()
    {
        IsOpen = false;

        if (Result == DialogResult.None)
            Result = DialogResult.Cancel;

        DialogResult result = Result;

        var completion = _completion;
        var callback = _callback;

        _completion = null;
        _callback = null;
        _owner = null;

        base.NotifyClosed();

        try
        {
            callback?.Invoke(result);
        }
        finally
        {
            completion?.TrySetResult(result);
        }
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        if (args.Input.Type == ControlInputEventType.KeyDown &&
            args.Input.Key == Keys.Escape)
        {
            Close();
            args.Handled = true;
            return;
        }

        if (args.Input.Type == ControlInputEventType.KeyDown &&
            args.Input.Key == Keys.Enter &&
            DefaultButton is Button button &&
            button.IsAvailable())
        {
            OnAccept();
            args.Handled = true;
            return;
        }

        base.OnInput(context, args);
    }

    /// <summary>
    /// Handles the default acceptance action of the dialog.
    /// </summary>
    /// <remarks>
    /// Activates the default button when available. Otherwise, the dialog
    /// completes with <see cref="DialogResult.OK"/>.
    /// Derived dialogs can override this method to customize acceptance.
    /// </remarks>
    protected virtual void OnAccept()
    {
        if (DefaultButton is Button button)
            button.PerformClick();
        else
            Complete(DialogResult.OK);
    }
}
