using Microsoft.Xna.Framework.Input;
using System;
using System.Threading;

namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Provides vibration control for a gamepad.
    /// </summary>
    public sealed class GamepadVibration : IVibration, IDisposable
    {
        private readonly object _syncRoot = new();

        private Timer? _stopTimer;
        private bool _isRunning;
        private bool _disposed;
        private int _lastIndex = -1;

        private float _lowFrequencyMotor = 1f;
        private float _highFrequencyMotor = 1f;
        private float _leftTriggerMotor;
        private float _rightTriggerMotor;

        /// <summary>
        /// Gets whether vibration is currently active.
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// Gets the maximum number of supported gamepad inputs.
        /// </summary>
        public int MaximumInputCount => GamePad.MaximumGamePadCount;

        /// <summary>
        /// Gets the input type associated with the vibration device.
        /// </summary>
        public InputType InputType => InputType.Gamepad;

        /// <summary>
        /// Gets or sets the low-frequency motor intensity in the range 0 to 1.
        /// </summary>
        public float LowFrequencyMotor
        {
            get => _lowFrequencyMotor;
            set
            {
                EnsureNotRunning();
                _lowFrequencyMotor = float.Clamp(value, 0f, 1f);
            }
        }

        /// <summary>
        /// Gets or sets the high-frequency motor intensity in the range 0 to 1.
        /// </summary>
        public float HighFrequencyMotor
        {
            get => _highFrequencyMotor;
            set
            {
                EnsureNotRunning();
                _highFrequencyMotor = float.Clamp(value, 0f, 1f);
            }
        }

        /// <summary>
        /// Gets or sets the left trigger motor intensity in the range 0 to 1.
        /// </summary>
        public float LeftTriggerMotor
        {
            get => _leftTriggerMotor;
            set
            {
                EnsureNotRunning();
                _leftTriggerMotor = float.Clamp(value, 0f, 1f);
            }
        }

        /// <summary>
        /// Gets or sets the right trigger motor intensity in the range 0 to 1.
        /// </summary>
        public float RightTriggerMotor
        {
            get => _rightTriggerMotor;
            set
            {
                EnsureNotRunning();
                _rightTriggerMotor = float.Clamp(value, 0f, 1f);
            }
        }

        /// <summary>
        /// Determines whether vibration is supported by the specified gamepad.
        /// </summary>
        /// <param name="inputIndex">The gamepad input index.</param>
        /// <returns>
        /// <see langword="true"/> if the gamepad is connected; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool IsSupported(int inputIndex = -1)
        {
            if ((uint)inputIndex >= MaximumInputCount)
                return false;

            return GamePad.GetState(inputIndex).IsConnected;
        }

        /// <summary>
        /// Starts vibration on the specified gamepad for the specified duration.
        /// </summary>
        /// <param name="duration">The vibration duration.</param>
        /// <param name="inputIndex">The gamepad input index.</param>
        public void Run(TimeSpan duration, int inputIndex = -1)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if ((uint)inputIndex >= MaximumInputCount)
                return;

            if (!GamePad.GetState(inputIndex).IsConnected)
                return;

            lock (_syncRoot)
            {
                StopCore();

                GamePad.SetVibration(
                    inputIndex,
                    _lowFrequencyMotor,
                    _highFrequencyMotor,
                    _leftTriggerMotor,
                    _rightTriggerMotor);

                _lastIndex = inputIndex;
                _isRunning = true;

                _stopTimer = new Timer(
                    static state => ((GamepadVibration)state!).Stop(),
                    this,
                    duration,
                    Timeout.InfiniteTimeSpan);
            }
        }

        /// <summary>
        /// Stops the currently active vibration.
        /// </summary>
        public void Stop()
        {
            lock (_syncRoot)
                StopCore();
        }

        /// <summary>
        /// Releases resources used by the vibration controller.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            lock (_syncRoot)
            {
                StopCore();
                _disposed = true;
            }
        }

        private void StopCore()
        {
            _stopTimer?.Dispose();
            _stopTimer = null;

            if (_isRunning && _lastIndex >= 0)
                GamePad.SetVibration(_lastIndex, 0f, 0f, 0f, 0f);

            _lastIndex = -1;
            _isRunning = false;
        }

        private void EnsureNotRunning()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_isRunning)
                throw new InvalidOperationException(
                    "Motor intensity cannot be changed while vibration is running.");
        }
    }
}