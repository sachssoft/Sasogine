using System;

namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Defines vibration control for an input device.
    /// </summary>
    public interface IVibration
    {
        /// <summary>
        /// Gets whether vibration is currently active.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets the maximum number of supported input devices.
        /// </summary>
        int MaximumInputCount { get; }

        /// <summary>
        /// Gets the type of input device supported by this vibration controller.
        /// </summary>
        InputType InputType { get; }

        /// <summary>
        /// Determines whether vibration is supported by the specified input device.
        /// </summary>
        /// <param name="inputIndex">The input device index.</param>
        /// <returns>
        /// <see langword="true"/> if vibration is supported; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        bool IsSupported(int inputIndex = -1);

        /// <summary>
        /// Starts vibration on the specified input device for the specified duration.
        /// </summary>
        /// <param name="duration">The vibration duration.</param>
        /// <param name="inputIndex">The input device index.</param>
        void Run(TimeSpan duration, int inputIndex = -1);

        /// <summary>
        /// Stops the currently active vibration.
        /// </summary>
        void Stop();
    }
}