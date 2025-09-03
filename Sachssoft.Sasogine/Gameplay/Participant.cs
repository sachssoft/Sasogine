using Sachssoft.Sasogine.Interactions;
using System;

namespace Sachssoft.Sasogine.Gameplay
{
    public abstract class Participant
    {

        private readonly IInteraction _interaction;
        private readonly object _axisInput;
        private TimeSpan _elapsed_time = TimeSpan.Zero;
        private TimeSpan _elapsed_time_completed = TimeSpan.Zero;
        private ParticipantState _state;

        public Participant()
        {
        }

        public Interaction<TInteractionEnum> GetInteraction<TInteractionEnum>() where TInteractionEnum : unmanaged, Enum
            => (Interaction<TInteractionEnum>)_interaction;

        public AxisInput<TAxisInputEnum> GetAxisInput<TAxisInputEnum>() where TAxisInputEnum : unmanaged, Enum
            => (AxisInput<TAxisInputEnum>)_axisInput;

        public ParticipantState State
        {
            get => _state;
            protected set => _state = value;
        }

        public bool Focus { get; set; }

        public TimeSpan ElapsedTime => _elapsed_time;

        public TimeSpan ElapsedTimeCompleted => _elapsed_time_completed;

        public virtual void Update(TimeSpan elapsed_time)
        {
            _elapsed_time = elapsed_time;
            _interaction.Update();
        }

        public virtual void Finish()
        {
            _state = ParticipantState.Finished;
            _elapsed_time_completed = _elapsed_time;
        }
    }
}
