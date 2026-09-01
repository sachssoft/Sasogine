using Sachssoft.Sasogine.Scenes;
using System;

namespace Sachssoft.Sasogine.Experimental.Input
{
    /// <summary>
    /// Defines a non-generic interface for managing and querying interaction states.
    /// </summary>
    public interface IInteraction
    {
        /// <summary>
        /// Sets the specified interaction to the pressed state.
        /// </summary>
        /// <param name="interaction">
        /// The interaction to press.
        /// </param>
        void Press(ulong interaction);

        /// <summary>
        /// Sets the specified interactions to the pressed state.
        /// </summary>
        /// <param name="interactions">
        /// The interactions to press.
        /// </param>
        void Press(params ulong[] interactions);

        /// <summary>
        /// Sets the specified interaction to the released state.
        /// </summary>
        /// <param name="interaction">
        /// The interaction to release.
        /// </param>
        void Release(ulong interaction);

        /// <summary>
        /// Sets the specified interactions to the released state.
        /// </summary>
        /// <param name="interactions">
        /// The interactions to release.
        /// </param>
        void Release(params ulong[] interactions);

        /// <summary>
        /// Determines whether the specified interaction is currently pressed.
        /// </summary>
        /// <param name="interaction">
        /// The interaction to query.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the interaction is currently pressed;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        bool IsPressed(ulong interaction);

        /// <summary>
        /// Determines whether the specified interaction was just pressed.
        /// </summary>
        /// <param name="interaction">
        /// The interaction to query.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the interaction was just pressed;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        bool WasJustPressed(ulong interaction);

        /// <summary>
        /// Determines whether the specified interaction was just released.
        /// </summary>
        /// <param name="interaction">
        /// The interaction to query.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the interaction was just released;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        bool WasJustReleased(ulong interaction);

        /// <summary>
        /// Updates the interaction state using the specified scene update context.
        /// </summary>
        /// <param name="context">
        /// Provides information about the current scene update.
        /// </param>
        void Update(SceneUpdateContext context);

        /// <summary>
        /// Clears all currently active interaction states.
        /// </summary>
        void Clear();

        /// <summary>
        /// Invokes the specified action for each currently pressed interaction.
        /// </summary>
        /// <param name="action">
        /// The action to invoke for each pressed interaction.
        /// </param>
        void ForEachPressed(Action<ulong> action);

        /// <summary>
        /// Invokes the specified action for each interaction that was just released.
        /// </summary>
        /// <param name="action">
        /// The action to invoke for each just-released interaction.
        /// </param>
        void ForEachJustReleased(Action<ulong> action);

        /// <summary>
        /// Invokes the specified action for each interaction that was just pressed.
        /// </summary>
        /// <param name="action">
        /// The action to invoke for each just-pressed interaction.
        /// </param>
        void ForEachJustPressed(Action<ulong> action);
    }
}