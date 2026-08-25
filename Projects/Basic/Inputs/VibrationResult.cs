namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Result of a vibration request.
    /// </summary>
    public readonly struct VibrationResult
    {
        /// <summary>
        /// True if the device supports vibration.
        /// </summary>
        public bool IsSupported { get; init; }

        /// <summary>
        /// True if the vibration command was successfully sent.
        /// </summary>
        public bool IsVibrated { get; init; }
    }
}
