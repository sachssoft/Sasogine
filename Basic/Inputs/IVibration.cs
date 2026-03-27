using System;

namespace Sachssoft.Sasogine.Inputs
{
    /// <summary>
    /// Interface to execute a vibration command on a device.
    /// Simple executor: triggers the vibration for a given duration.
    /// Motor/intensity details are device-specific and handled by the implementation.
    /// </summary>
    public interface IVibration
    {
        /// <summary>True if vibration is currently active.</summary>
        bool IsRunning { get; }

        /// <summary>Maximum number of supported input devices (e.g., number of Gamepads).</summary>
        int MaximumInputCount { get; }

        /// <summary>Type of input device this vibration implementation supports (Gamepad, Touch, etc.).</summary>
        InputType InputType { get; }

        /// <summary>
        /// Returns true if this device supports vibration at the given index.
        /// </summary>
        bool IsSupported(int inputIndex = -1);

        /// <summary>
        /// Executes vibration on the specified input device for a given duration.
        /// The implementation decides which motors/outputs to use.
        /// </summary>
        /// <param name="duration">Duration of the vibration.</param>
        /// <param name="inputIndex">Index of the device (-1 = all connected devices).</param>
        void Run(TimeSpan duration, int inputIndex = -1);

        /// <summary>Stops vibration immediately.</summary>
        void Stop();
    }
}