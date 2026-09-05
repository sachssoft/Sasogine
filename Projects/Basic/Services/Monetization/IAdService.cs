using System;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Services.Monetization;

/// <summary>
/// Provides access to platform-specific advertising services.
/// </summary>
public interface IAdService
{
    /// <summary>
    /// Gets a value indicating whether advertising is supported
    /// on the current platform.
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Shows an interstitial advertisement when one is available.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the advertisement was shown successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> ShowInterstitialAsync();

    /// <summary>
    /// Shows a rewarded advertisement when one is available.
    /// </summary>
    /// <param name="onRewardGranted">
    /// An optional callback invoked when the reward has been granted.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the rewarded advertisement was completed
    /// successfully; otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> ShowRewardedAdAsync(
        Func<Task>? onRewardGranted = null);

    /// <summary>
    /// Gets a value indicating whether an interstitial advertisement
    /// is currently ready to be shown.
    /// </summary>
    bool IsInterstitialReady { get; }

    /// <summary>
    /// Gets a value indicating whether a rewarded advertisement
    /// is currently ready to be shown.
    /// </summary>
    bool IsRewardedReady { get; }

    /// <summary>
    /// Occurs when a rewarded advertisement has been completed
    /// and the reward has been granted.
    /// </summary>
    event Action? RewardedCompleted;

    /// <summary>
    /// Initializes the advertising service.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous initialization operation.
    /// </returns>
    Task InitializeAsync();
}