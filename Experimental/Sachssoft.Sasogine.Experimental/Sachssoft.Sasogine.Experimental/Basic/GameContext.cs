using System.Diagnostics;

namespace Sachssoft.Sasogine.Experimental
{
    public class GameContext
    {
        private readonly GameApplication _application;
        private Stopwatch _stopwatch;
        private TimeSpan _lastElapsed;

        public GameContext(GameApplication application)
        {
            _application = application;
            _stopwatch = Stopwatch.StartNew();
            _lastElapsed = TimeSpan.Zero;
        }

        public GameApplication Application => _application;

        // Zeit seit letztem Invalidate
        public TimeSpan DeltaTime { get; private set; }     // Zeit seit letztem Frame
        public TimeSpan TotalTime { get; private set; }     // Zeit seit Start
        public int FrameCount { get; private set; }         // Optional, Zähler

        internal protected virtual GameContext Invalidate()
        {
            TimeSpan currentElapsed = _stopwatch.Elapsed;
            DeltaTime = currentElapsed - _lastElapsed;
            _lastElapsed = currentElapsed;
            TotalTime = currentElapsed;
            FrameCount++;
            return this;
        }
    }
}