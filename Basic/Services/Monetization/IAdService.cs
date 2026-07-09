using System;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Services.Monetization;

public interface IAdService
{
    /// <summary>
    /// Gibt an, ob Werbeanzeigen unterstützt werden (Plattformprüfung).
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Zeigt ein Interstitial (Ganzseitenanzeige) an, wenn verfügbar.
    /// </summary>
    Task<bool> ShowInterstitialAsync();

    /// <summary>
    /// Zeigt ein Rewarded Video an und ruft Callback auf Erfolg auf.
    /// </summary>
    Task<bool> ShowRewardedAdAsync(Func<Task>? onRewardGranted = null);

    /// <summary>
    /// Gibt an, ob ein Interstitial verfügbar ist.
    /// </summary>
    bool IsInterstitialReady { get; }

    /// <summary>
    /// Gibt an, ob ein Rewarded-Ad verfügbar ist.
    /// </summary>
    bool IsRewardedReady { get; }

    /// <summary>
    /// Optional: Wird aufgerufen, wenn ein Rewarded-Ad abgeschlossen wurde.
    /// </summary>
    event Action? RewardedCompleted;

    /// <summary>
    /// Optional: Initialisiert das Werbenetzwerk (z. B. beim Start).
    /// </summary>
    Task InitializeAsync();
}
