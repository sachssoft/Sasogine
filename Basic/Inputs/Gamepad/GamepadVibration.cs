using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Inputs;
using System;
using System.Threading;

namespace Sachssoft.Sasogine.Platform.Windows
{
    /// <summary>
    /// Executes vibration on a single Gamepad on Windows using MonoGame XInput.
    /// Supports low/high frequency motors and optional left/right trigger motors.
    /// </summary>
    public class GamepadVibration : IVibration
    {
        private bool _isRunning;
        private int _lastIndex = -1;
        private Timer? _stopTimer;

        // Backing fields for motor intensities
        private float _lowFrequencyMotor = 1f;
        private float _highFrequencyMotor = 1f;
        private float _leftTriggerMotor = 0f;
        private float _rightTriggerMotor = 0f;

        /// <summary>True if vibration is currently active.</summary>
        public bool IsRunning => _isRunning;

        public int MaximumInputCount => GamePad.MaximumGamePadCount;

        public InputType InputType { get; } = InputType.Gamepad;

        /// <summary>Left / low-frequency motor intensity [0..1]. Cannot change while running.</summary>
        public float LowFrequencyMotor
        {
            get => _lowFrequencyMotor;
            set
            {
                if (_isRunning)
                    throw new InvalidOperationException("Cannot change LowFrequencyMotor while vibration is running.");
                _lowFrequencyMotor = float.Clamp(value, 0f, 1f);
            }
        }

        /// <summary>Right / high-frequency motor intensity [0..1]. Cannot change while running.</summary>
        public float HighFrequencyMotor
        {
            get => _highFrequencyMotor;
            set
            {
                if (_isRunning)
                    throw new InvalidOperationException("Cannot change HighFrequencyMotor while vibration is running.");
                _highFrequencyMotor = float.Clamp(value, 0f, 1f);
            }
        }

        /// <summary>Left trigger motor intensity [0..1]. Optional, cannot change while running.</summary>
        public float LeftTriggerMotor
        {
            get => _leftTriggerMotor;
            set
            {
                if (_isRunning)
                    throw new InvalidOperationException("Cannot change LeftTriggerMotor while vibration is running.");
                _leftTriggerMotor = float.Clamp(value, 0f, 1f);
            }
        }

        /// <summary>Right trigger motor intensity [0..1]. Optional, cannot change while running.</summary>
        public float RightTriggerMotor
        {
            get => _rightTriggerMotor;
            set
            {
                if (_isRunning)
                    throw new InvalidOperationException("Cannot change RightTriggerMotor while vibration is running.");
                _rightTriggerMotor = float.Clamp(value, 0f, 1f);
            }
        }

        /// <summary>
        /// Checks if vibration is supported on the specified Gamepad.
        /// </summary>
        public bool IsSupported(int inputIndex = -1)
        {
            if (inputIndex < 0 || inputIndex >= MaximumInputCount)
                return false;

            var state = GamePad.GetState(inputIndex);
            return state.IsConnected;
        }

        /// <summary>
        /// Executes vibration on the specified Gamepad using the current motor settings
        /// for a given duration.
        /// </summary>
        public void Run(TimeSpan duration, int inputIndex = -1)
        {
            if (inputIndex < 0 || inputIndex >= MaximumInputCount)
                return;

            var state = GamePad.GetState(inputIndex);
            if (!state.IsConnected)
                return;

            // Clamp motor values to [0..1]
            float low = float.Clamp(_lowFrequencyMotor, 0f, 1f);
            float high = float.Clamp(_highFrequencyMotor, 0f, 1f);
            float leftTrig = float.Clamp(_leftTriggerMotor, 0f, 1f);
            float rightTrig = float.Clamp(_rightTriggerMotor, 0f, 1f);

            // Start vibration (low/high + triggers)
            GamePad.SetVibration(inputIndex, low, high, leftTrig, rightTrig);

            _lastIndex = inputIndex;
            _isRunning = true;

            // Dispose previous timer safely
            _stopTimer?.Dispose();

            // Create new timer to stop vibration after duration
            _stopTimer = new Timer(_ =>
            {
                Stop();
            }, null, duration, Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// Stops vibration on the last executed Gamepad.
        /// </summary>
        public void Stop()
        {
            if (_isRunning && _lastIndex >= 0)
            {
                // Stop all motors
                GamePad.SetVibration(_lastIndex, 0f, 0f, 0f, 0f);

                // Dispose the timer safely
                _stopTimer?.Dispose();
                _stopTimer = null;

                _lastIndex = -1;
                _isRunning = false;
            }
        }
    }
}