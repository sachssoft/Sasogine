namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Represents the result of a vibration request.
    /// </summary>
    public readonly struct VibrationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VibrationResult"/> struct.
        /// </summary>
        /// <param name="isSupported">
        /// Indicates whether the device supports vibration.
        /// </param>
        /// <param name="isVibrated">
        /// Indicates whether the vibration command was successfully sent.
        /// </param>
        public VibrationResult(bool isSupported, bool isVibrated)
        {
            IsSupported = isSupported;
            IsVibrated = isVibrated;
        }

        /// <summary>
        /// Gets whether the device supports vibration.
        /// </summary>
        public bool IsSupported { get; }

        /// <summary>
        /// Gets whether the vibration command was successfully sent.
        /// </summary>
        public bool IsVibrated { get; }
    }
}