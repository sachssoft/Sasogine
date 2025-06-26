using System.Runtime.Serialization;

namespace sachssoft.Sasogine.Animation;

[DataContract]
public class RotationAnimation : AnimationBase
{
    private float _currentRotation;

    // Die Rotation in Grad wird aufgerufen, basierend auf der verstrichenen Zeit
    protected override float AddRotationDegreeOverride(float elapsed_time)
    {
        // Berechnet die Rotationsänderung basierend auf der verstrichenen Zeit
        _currentRotation += elapsed_time * -Speed;

        // Hier könnte man eine Modulo-Operation hinzufügen, um den Wert innerhalb des Bereichs [0, 360] zu halten
        if (_currentRotation >= 360f)
        {
            _currentRotation -= 360f;
        }
        else if (_currentRotation < 0f)
        {
            _currentRotation += 360f;
        }

        return _currentRotation;
    }
}
