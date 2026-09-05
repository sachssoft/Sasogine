using System;

namespace Sachssoft.Sasogine.Gameplay;

/// <summary>
/// Represents a participant that can take part in a gameplay session
/// and tracks its current state and elapsed time.
/// </summary>
public abstract class Participant
{
    private TimeSpan _elapsedTime;
    private TimeSpan _elapsedTimeCompleted;
    private ParticipantState _state;

    /// <summary>
    /// Gets the current state of the participant.
    /// </summary>
    public ParticipantState State
    {
        get => _state;
        protected set => _state = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the participant currently has focus.
    /// </summary>
    public bool Focus { get; set; }

    /// <summary>
    /// Gets the elapsed time associated with the most recent update.
    /// </summary>
    public TimeSpan ElapsedTime => _elapsedTime;

    /// <summary>
    /// Gets the elapsed time recorded when the participant finished.
    /// </summary>
    public TimeSpan ElapsedTimeCompleted => _elapsedTimeCompleted;

    /// <summary>
    /// Updates the participant using the specified elapsed time.
    /// </summary>
    /// <param name="elapsedTime">
    /// The elapsed time for the current update cycle.
    /// </param>
    public virtual void Update(TimeSpan elapsedTime)
    {
        _elapsedTime = elapsedTime;
    }

    /// <summary>
    /// Marks the participant as finished and records its elapsed time.
    /// </summary>
    public virtual void Finish()
    {
        State = ParticipantState.Finished;
        _elapsedTimeCompleted = _elapsedTime;
    }
}