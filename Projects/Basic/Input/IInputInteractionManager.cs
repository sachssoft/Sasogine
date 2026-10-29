using Sachssoft.Engine.Scenes;

namespace Sachssoft.Engine.Input;

/// <summary>
/// Provides button-independent access to an input interaction manager.
/// </summary>
/// <remarks>
/// Configure button bindings through the concrete manager before using this interface
/// to update or clear managers for different input devices and local players.
/// </remarks>
public interface IInputInteractionManager
{
    /// <summary>
    /// Gets or sets the listener that receives input interaction notifications.
    /// </summary>
    IInputInteractionListener? Listener
    {
        get;
        set;
    }

    /// <summary>
    /// Updates the input interactions for the current scene update.
    /// </summary>
    /// <param name="context">The current scene update context.</param>
    void Update(SceneUpdateContext context);

    /// <summary>
    /// Removes all registered combinations and active combinations without invoking release actions.
    /// </summary>
    void ClearCombinations();

    /// <summary>
    /// Clears the currently active combinations while retaining their registrations.
    /// </summary>
    /// <param name="triggerReleaseActions">
    /// Whether to invoke release actions and notify the listener for active combinations.
    /// </param>
    void ClearActiveCombinations(bool triggerReleaseActions = true);

    /// <summary>
    /// Removes all registered button sequences.
    /// </summary>
    void ClearSequences();

    /// <summary>
    /// Removes all combinations and sequences without invoking release actions.
    /// </summary>
    void Clear();
}
