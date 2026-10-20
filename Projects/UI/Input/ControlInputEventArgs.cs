using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;

namespace Sachssoft.Engine.Controls.Input;
/// <summary>Borrowed routing state valid only during dispatch. Copy Input to retain event data.</summary>
public sealed class ControlInputEventArgs
{
    internal ControlInputEventArgs(ControlInputEvent input, Control source)
    {
        Input = input;
        Source = source;
        CurrentTarget = source;
    }

    /// <summary>Gets the original event data.</summary>
    public ControlInputEvent Input { get; private set; }
    /// <summary>Gets the original target.</summary>
    public Control Source { get; private set; }
    /// <summary>Gets the node currently processing this event.</summary>
    public Control CurrentTarget { get; internal set; }
    /// <summary>Gets or sets whether bubbling should stop.</summary>
    public bool Handled { get; set; }

    internal ControlInputEventArgs Reset(ControlInputEvent input, Control source)
    {
        Input = input;
        Source = CurrentTarget = source;
        Handled = false;
        return this;
    }
}
