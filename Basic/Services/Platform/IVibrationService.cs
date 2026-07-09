using Sachssoft.Sasogine.Input;

namespace Sachssoft.Sasogine.Services.Platform
{
    /// <summary>
    /// Service for providing haptic feedback / device vibration.
    /// Desktop: Gamepads.
    /// Mobile: native vibration APIs (iOS/Android).
    /// </summary>
    public interface IVibrationService
    {
        /// <summary>Registers a vibration device (Gamepad, Mobile, etc.)</summary>
        void AddDevice(IVibration vibration);

        /// <summary>
        /// Executes vibration on a device of the specified InputType.
        /// </summary>
        /// <param name="inputType">Type of device (Gamepad, Touch, etc.)</param>
        /// <param name="inputIndex">Device index (default -1 = first available)</param>
        /// <param name="duration">Duration in milliseconds</param>
        /// <returns>Result of vibration attempt (success/failure)</returns>
        VibrationResult Vibrate(InputType inputType, int inputIndex = -1, int duration = 200);

        /// <summary>Stops all currently running vibration devices.</summary>
        void Stop();
    }
}