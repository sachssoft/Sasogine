using System;

namespace Sachssoft.Sasogine.Gameplay;

/// <summary>
/// Provides the elapsed game time since the start of the gameplay session.
/// </summary>
public interface IGameplayTime
{
    /// <summary>
    /// Gets the total elapsed game time since the start of the gameplay.
    /// </summary>
    TimeSpan ElapsedGameTime { get; }
}
