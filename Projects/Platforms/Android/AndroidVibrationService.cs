using Sachssoft.Engine.Input;
using Sachssoft.Engine.Services.Platform;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Platform.Android;

/// <summary>
/// Android implementation of <see cref="IVibrationService"/>.
/// </summary>
public sealed class AndroidVibrationService : IVibrationService
{
    private readonly List<IVibration> _devices = new();

    /// <inheritdoc />
    public void AddDevice(IVibration vibration)
    {
        ArgumentNullException.ThrowIfNull(vibration);
        if (!_devices.Contains(vibration))
            _devices.Add(vibration);
    }

    /// <inheritdoc />
    public VibrationResult Vibrate(InputType inputType, int inputIndex = -1, int duration = 200)
    {
        if (inputIndex < -1)
            throw new ArgumentOutOfRangeException(nameof(inputIndex));
        if (duration < 0)
            throw new ArgumentOutOfRangeException(nameof(duration));

        IVibration? selectedDevice = null;
        var selectedIndex = -1;

        foreach (var device in _devices)
        {
            if (device.InputType != inputType)
                continue;

            if (inputIndex >= 0)
            {
                if (inputIndex < device.MaximumInputCount && device.IsSupported(inputIndex))
                {
                    selectedDevice = device;
                    selectedIndex = inputIndex;
                    break;
                }
                continue;
            }

            for (var index = 0; index < device.MaximumInputCount; index++)
            {
                if (!device.IsSupported(index))
                    continue;
                selectedDevice = device;
                selectedIndex = index;
                break;
            }

            if (selectedDevice is not null)
                break;
        }

        if (selectedDevice is null)
            return new VibrationResult(isSupported: false, isVibrated: false);
        if (selectedDevice.IsRunning)
            return new VibrationResult(isSupported: true, isVibrated: false);

        try
        {
            selectedDevice.Run(TimeSpan.FromMilliseconds(duration), selectedIndex);
            return new VibrationResult(isSupported: true, isVibrated: true);
        }
        catch
        {
            return new VibrationResult(isSupported: true, isVibrated: false);
        }
    }

    /// <inheritdoc />
    public void Stop()
    {
        foreach (var device in _devices)
        {
            if (device.IsRunning)
                device.Stop();
        }
    }
}
