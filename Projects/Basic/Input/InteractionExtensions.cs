using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Input;

/// <summary>
/// Provides extension methods for binding keyboard and mouse input
/// to strongly typed interaction values.
/// </summary>
public static class InteractionExtensions
{
    /// <summary>
    /// Binds a keyboard key to an interaction value, pressing the
    /// interaction when the key is pressed and releasing it when
    /// the key is released.
    /// </summary>
    /// <typeparam name="TInteractionEnum">
    /// The enumeration type used to identify interaction values.
    /// </typeparam>
    /// <param name="manager">
    /// The keyboard interaction manager to which the binding is added.
    /// </param>
    /// <param name="button">
    /// The keyboard key that triggers the interaction.
    /// </param>
    /// <param name="interaction">
    /// The interaction whose state is controlled by the key.
    /// </param>
    /// <param name="value">
    /// The interaction value pressed and released by the binding.
    /// </param>
    public static void Bind<TInteractionEnum>(
        this KeyboardInteractionManager manager,
        Keys button,
        Interaction<TInteractionEnum> interaction,
        TInteractionEnum value)
        where TInteractionEnum : unmanaged, Enum
    {
        manager.Add(
            button,
            () => interaction.Press(value),
            () => interaction.Release(value));
    }

    /// <summary>
    /// Binds a combination of keyboard keys to an interaction value,
    /// pressing the interaction when the combination is activated and
    /// releasing it when the combination is released.
    /// </summary>
    /// <typeparam name="TInteractionEnum">
    /// The enumeration type used to identify interaction values.
    /// </typeparam>
    /// <param name="manager">
    /// The keyboard interaction manager to which the binding is added.
    /// </param>
    /// <param name="buttons">
    /// The keyboard keys that form the required combination.
    /// </param>
    /// <param name="interaction">
    /// The interaction whose state is controlled by the key combination.
    /// </param>
    /// <param name="value">
    /// The interaction value pressed and released by the binding.
    /// </param>
    public static void BindCombination<TInteractionEnum>(
        this KeyboardInteractionManager manager,
        IEnumerable<Keys> buttons,
        Interaction<TInteractionEnum> interaction,
        TInteractionEnum value)
        where TInteractionEnum : unmanaged, Enum
    {
        manager.AddCombination(
            buttons,
            () => interaction.Press(value),
            () => interaction.Release(value));
    }

    /// <summary>
    /// Binds an ordered sequence of keyboard keys to an interaction value,
    /// pressing the interaction when the complete sequence is recognized.
    /// </summary>
    /// <typeparam name="TInteractionEnum">
    /// The enumeration type used to identify interaction values.
    /// </typeparam>
    /// <param name="manager">
    /// The keyboard interaction manager to which the binding is added.
    /// </param>
    /// <param name="sequences">
    /// The ordered keyboard keys that form the input sequence.
    /// </param>
    /// <param name="interaction">
    /// The interaction activated when the sequence is recognized.
    /// </param>
    /// <param name="value">
    /// The interaction value pressed when the sequence is recognized.
    /// </param>
    /// <param name="timeout">
    /// The optional maximum duration allowed for completing the sequence.
    /// A <see langword="null"/> value uses the interaction manager's
    /// default sequence timing behavior.
    /// </param>
    public static void BindSequence<TInteractionEnum>(
        this KeyboardInteractionManager manager,
        IList<Keys> sequences,
        Interaction<TInteractionEnum> interaction,
        TInteractionEnum value,
        TimeSpan? timeout = null)
        where TInteractionEnum : unmanaged, Enum
    {
        manager.AddSequence(
            sequences,
            () => interaction.Press(value),
            timeout);
    }

    /// <summary>
    /// Binds a mouse button to an interaction value, pressing the
    /// interaction when the button is pressed and releasing it when
    /// the button is released.
    /// </summary>
    /// <typeparam name="TInteractionEnum">
    /// The enumeration type used to identify interaction values.
    /// </typeparam>
    /// <param name="manager">
    /// The mouse interaction manager to which the binding is added.
    /// </param>
    /// <param name="button">
    /// The mouse button that triggers the interaction.
    /// </param>
    /// <param name="interaction">
    /// The interaction whose state is controlled by the mouse button.
    /// </param>
    /// <param name="value">
    /// The interaction value pressed and released by the binding.
    /// </param>
    public static void Bind<TInteractionEnum>(
        this MouseInteractionManager manager,
        MouseButton button,
        Interaction<TInteractionEnum> interaction,
        TInteractionEnum value)
        where TInteractionEnum : unmanaged, Enum
    {
        manager.Add(
            button,
            () => interaction.Press(value),
            () => interaction.Release(value));
    }
}