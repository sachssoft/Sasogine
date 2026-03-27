using Sachssoft.Sasogine.Experimental.Platform;
using System;

namespace Sachssoft.Sasogine.Experimental
{
    public abstract class GameApplication
    {
        private readonly IPlatformWindow _window;
        private readonly IPlatformBackend _backend;
        private readonly GameContext _context;

        private TimeSpan _fixedDeltaTime = TimeSpan.FromSeconds(1.0 / 60.0); // Standard 60Hz
        private TimeSpan _accumulator = TimeSpan.Zero;

        private bool _isRunning;

        public GameApplication(PlatformBase platform)
        {
            _window = platform.CreateWindow(ConfigureWindow());
            _backend = platform.CreateBackend(ConfigureBackend());
            _context = new GameContext(this);
        }

        /// <summary>
        /// Fixed Time Step für Updates (kann zur Laufzeit angepasst werden)
        /// </summary>
        public TimeSpan FixedDeltaTime
        {
            get => _fixedDeltaTime;
            set
            {
                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException(nameof(FixedDeltaTime), "Must be positive");
                _fixedDeltaTime = value;
            }
        }

        public void Run()
        {
            _isRunning = true;
            _window.Initialize();
            _backend.Initialize(_window);

            try
            {
                while (_isRunning)
                {
                    _window.Poll();
                    Tick(_context);
                    _backend.Present();
                }
            }
            finally
            {
                _backend.Dispose();
                _window.Close();
            }
        }

        protected abstract WindowConfiguration ConfigureWindow();
        protected abstract BackendConfiguration ConfigureBackend();

        protected virtual void Tick(GameContext context)
        {
            var ctx = context.Invalidate(); // DeltaTime aktualisiert
            _accumulator += ctx.DeltaTime;

            while (_accumulator >= FixedDeltaTime)
            {
                Update(ctx); // Update mit konstantem Zeitschritt
                _accumulator -= FixedDeltaTime;
            }

            Render(ctx); // Render so oft wie möglich
        }

        protected virtual void Update(GameContext context)
        {
            // Game logic
        }

        protected virtual void Render(GameContext context)
        {
            // Rendering
        }

        public void Exit()
        {
            _isRunning = false;
        }
    }
}