using Microsoft.Xna.Framework;

namespace sachssoft.Sasogine.Surface.Visuals.Controls;

internal interface ITransformable
{
    Vector2 ToLocal(Vector2 source);
    Vector2 ToGlobal(Vector2 pos);
}
