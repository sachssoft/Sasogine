using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Inputs;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Platform.Windows
{
    /// <summary>
    /// Windows implementation of IVibrationService using MonoGame XInput.
    /// Only vibrates a selected Gamepad (inputIndex >= 0). No action if inputIndex = -1.
    /// </summary>
    public class WindowsVibrationService : IVibrationService
    {
        private readonly List<IVibration> _devices = new List<IVibration>();

        /// <summary>Adds a vibration device to the service (avoids duplicates).</summary>
        public void AddDevice(IVibration vibration)
        {
            if (vibration == null) return;
            for (int i = 0; i < _devices.Count; i++)
            {
                if (ReferenceEquals(_devices[i], vibration)) return;
            }
            _devices.Add(vibration);
        }

        /// <summary>
        /// Vibrates the specified device by index. If inputIndex = -1, no vibration occurs.
        /// </summary>
        public VibrationResult Vibrate(InputType inputType, int inputIndex = -1, int duration = 200)
        {
            if (inputIndex < 0 || inputIndex >= GamePad.MaximumGamePadCount)
                throw new ArgumentOutOfRangeException(nameof(inputIndex),
                    $"Input index must be between 0 and {GamePad.MaximumGamePadCount - 1}.");

            IVibration? device = null;
            for (int i = 0; i < _devices.Count; i++)
            {
                IVibration d = _devices[i];
                if (d.InputType == inputType && d.IsSupported(inputIndex))
                {
                    device = d;
                    break;
                }
            }

            if (device == null)
            {
                return new VibrationResult
                {
                    IsSupported = false,
                    IsVibrated = false
                };
            }

            if (device.IsRunning)
            {
                return new VibrationResult
                {
                    IsSupported = true,
                    IsVibrated = false
                };
            }

            try
            {
                device.Run(TimeSpan.FromMilliseconds(duration), inputIndex);
                return new VibrationResult
                {
                    IsSupported = true,
                    IsVibrated = true
                };
            }
            catch
            {
                return new VibrationResult
                {
                    IsSupported = true,
                    IsVibrated = false
                };
            }
        }

        /// <summary>Stops all currently running vibration devices.</summary>
        public void Stop()
        {
            for (int i = 0; i < _devices.Count; i++)
            {
                IVibration d = _devices[i];
                if (d.IsRunning)
                    d.Stop();
            }
        }
    }
}