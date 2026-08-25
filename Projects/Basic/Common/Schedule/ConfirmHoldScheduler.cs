using System;

namespace Sachssoft.Sasogine.Common.Schedule
{
    public class RepeatWithDelayScheduler
    {
        private TimeSpan _elapsed = TimeSpan.Zero;
        private bool _isRunning = false;
        private bool _isFirstTick = true;

        /// <summary>
        /// Verzögerung nach der ersten Auslösung, bevor Wiederholungen starten.
        /// </summary>
        public TimeSpan RepeatDelay { get; set; } = TimeSpan.FromMilliseconds(300);

        /// <summary>
        /// Intervall für die Wiederholungen nach der Verzögerung.
        /// </summary>
        public TimeSpan RepeatInterval { get; set; } = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// Gibt an, ob der Scheduler läuft.
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// Wenn true, wird nach dem ersten Auslösen (Sofort-Trigger) 
        /// die Wiederholung mit RepeatDelay und RepeatInterval ausgeführt.
        /// Wenn false, wird nur einmal beim Start ausgelöst.
        /// </summary>
        public bool IsRecurring { get; set; } = true;

        /// <summary>
        /// Event, das ausgelöst wird bei Auslösung (Sofort-Trigger und Wiederholungen).
        /// </summary>
        public event Action? Triggered;

        /// <summary>
        /// Event, das ausgelöst wird, wenn der Scheduler gestoppt wird.
        /// </summary>
        public event Action? Stopped;

        /// <summary>
        /// Startet den Scheduler, löst sofort aus.
        /// </summary>
        public void Start()
        {
            if (!_isRunning)
            {
                _elapsed = TimeSpan.Zero;
                _isRunning = true;
                _isFirstTick = true;

                Triggered?.Invoke();

                if (!IsRecurring)
                    Stop();
            }
        }

        /// <summary>
        /// Stoppt den Scheduler.
        /// </summary>
        public void Stop()
        {
            if (_isRunning)
            {
                _isRunning = false;
                _elapsed = TimeSpan.Zero;
                Stopped?.Invoke();
            }
        }

        /// <summary>
        /// Update muss mit verstrichener Zeit aufgerufen werden.
        /// </summary>
        public void Update(TimeSpan elapsed)
        {
            if (!_isRunning || !IsRecurring)
                return;

            _elapsed += elapsed;

            if (_isFirstTick)
            {
                // Warte RepeatDelay bis erste Wiederholung
                if (_elapsed >= RepeatDelay)
                {
                    _elapsed = TimeSpan.Zero;
                    _isFirstTick = false;
                    Triggered?.Invoke();
                }
            }
            else
            {
                // Regelmäßige Wiederholungen mit RepeatInterval
                if (_elapsed >= RepeatInterval)
                {
                    _elapsed = TimeSpan.Zero;
                    Triggered?.Invoke();
                }
            }
        }
    }
}
