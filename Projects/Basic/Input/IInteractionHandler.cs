using System;

namespace Sachssoft.Sasogine.Input;

/// <summary>
/// Defines a listener that receives notifications for input interactions.
/// </summary>
public interface IInputInteractionListener
{
    /// <summary>
    /// Called when one or more interactions become pressed.
    /// </summary>
    /// <param name="buttonType">
    /// The enum type used for the input buttons.
    /// </param>
    /// <param name="interactions">
    /// The interaction identifiers.
    /// </param>
    void Press(Type buttonType, params ulong[] interactions);

    /// <summary>
    /// Called when one or more interactions become released.
    /// </summary>
    /// <param name="buttonType">
    /// The enum type used for the input buttons.
    /// </param>
    /// <param name="interactions">
    /// The interaction identifiers.
    /// </param>
    void Release(Type buttonType, params ulong[] interactions);

    /// <summary>
    /// Called when one or more interactions are triggered.
    /// </summary>
    /// <param name="buttonType">
    /// The enum type used for the input buttons.
    /// </param>
    /// <param name="interactions">
    /// The interaction identifiers.
    /// </param>
    void Trigger(Type buttonType, params ulong[] interactions);
}