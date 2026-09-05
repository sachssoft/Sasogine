namespace Sachssoft.Sasogine.Gameplay;

/// <summary>
/// Defines the possible states of a participant throughout the game lifecycle.
/// </summary>
public enum ParticipantState
{
    /// <summary>
    /// No participant state is currently assigned.
    /// </summary>
    None,

    /// <summary>
    /// The participant is establishing a connection to a server or host.
    /// </summary>
    Connecting,

    /// <summary>
    /// The participant is being authenticated.
    /// </summary>
    Authenticating,

    /// <summary>
    /// The participant is currently in a lobby.
    /// </summary>
    InLobby,

    /// <summary>
    /// The participant is waiting for a matchmaking result.
    /// </summary>
    Matchmaking,

    /// <summary>
    /// The participant is waiting to confirm readiness.
    /// </summary>
    ReadyCheck,

    /// <summary>
    /// The participant is loading or synchronizing data required to start.
    /// </summary>
    Loading,

    /// <summary>
    /// The participant is ready to start.
    /// </summary>
    Ready,

    /// <summary>
    /// The participant is waiting for the game, round, or other participants.
    /// </summary>
    Waiting,

    /// <summary>
    /// The participant is currently in a selection phase.
    /// </summary>
    Selection,

    /// <summary>
    /// The participant is being spawned into the game.
    /// </summary>
    Spawning,

    /// <summary>
    /// The participant is active and taking part in gameplay.
    /// </summary>
    Active,

    /// <summary>
    /// The participant is inactive or temporarily not providing input.
    /// </summary>
    Idle,

    /// <summary>
    /// The participant is currently paused.
    /// </summary>
    Paused,

    /// <summary>
    /// The participant is interacting with an object or interface.
    /// </summary>
    Interacting,

    /// <summary>
    /// The participant is performing an explicit gameplay action.
    /// </summary>
    PerformingAction,

    /// <summary>
    /// The participant is currently trading or exchanging items.
    /// </summary>
    Trading,

    /// <summary>
    /// The participant is hidden from normal visibility.
    /// </summary>
    Hidden,

    /// <summary>
    /// The participant is temporarily invincible.
    /// </summary>
    Invincible,

    /// <summary>
    /// The participant is temporarily unable to act.
    /// </summary>
    Stunned,

    /// <summary>
    /// The participant is disabled by gameplay rules or restrictions.
    /// </summary>
    Disabled,

    /// <summary>
    /// The participant has been killed but may still be able to respawn.
    /// </summary>
    Killed,

    /// <summary>
    /// The participant is currently respawning.
    /// </summary>
    Respawning,

    /// <summary>
    /// The participant has been permanently eliminated.
    /// </summary>
    Eliminated,

    /// <summary>
    /// The participant has completed the current match, level, or objective.
    /// </summary>
    Finished,

    /// <summary>
    /// The participant is observing gameplay as a spectator.
    /// </summary>
    Spectating,

    /// <summary>
    /// The participant is in the post-game phase.
    /// </summary>
    PostGame,

    /// <summary>
    /// The participant is receiving rewards, experience, or other results.
    /// </summary>
    Rewarding,

    /// <summary>
    /// The participant is reviewing results, statistics, or replay information.
    /// </summary>
    Reviewing,

    /// <summary>
    /// The participant has completely exited the game session.
    /// </summary>
    Exited,

    /// <summary>
    /// The participant is synchronizing state or data.
    /// </summary>
    Syncing,

    /// <summary>
    /// The participant is experiencing network latency or delayed synchronization.
    /// </summary>
    Lagging,

    /// <summary>
    /// The participant state is no longer synchronized with the authoritative state.
    /// </summary>
    OutOfSync,

    /// <summary>
    /// The participant has lost its network connection.
    /// </summary>
    Disconnected,

    /// <summary>
    /// The participant is attempting to restore a lost connection.
    /// </summary>
    Reconnecting,

    /// <summary>
    /// The participant voluntarily left the game or session.
    /// </summary>
    Left,

    /// <summary>
    /// The participant is banned from participating.
    /// </summary>
    Banned,

    /// <summary>
    /// The participant is temporarily suspended from participating.
    /// </summary>
    Suspended
}