using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace sachssoft.Sasogine.Animation;

public class PositionAnimation : AnimationBase
{
    private float _progress = 0f; // Besser lesbarer Name
    private float _direction = 1f; // Richtung der Bewegung (vorwärts oder rückwärts)
    private Vector2 _distance = Vector2.Zero;

    protected override Vector2 AddPositionOverride(float elapsed_time)
    {
        // Berechnet den Fortschritt basierend auf der Zeit und Geschwindigkeit
        var progressIncrement = (elapsed_time * Speed) / 100f;

        // Richtungsumkehr, wenn der Fortschritt 0 oder 1 erreicht
        if (_progress >= 1f)
        {
            _direction = -1f; // Kehrt die Richtung um
        }
        else if (_progress <= 0f)
        {
            _direction = 1f; // Kehrt die Richtung um
        }

        // Aktualisiere den Fortschritt
        _progress += progressIncrement * _direction;

        // Berechne die Position basierend auf der Entfernung und dem Fortschritt
        var newPosition = _distance * _progress;

        return newPosition;
    }

    // Startverzögerung
    public virtual Vector2 Distance
    {
        get => _distance;
        set => RaiseAndSetIfChanged(ref _distance, value);
    }
}
