using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Surface.Visuals;

internal interface ITransformable
{
    Vector2 ToLocal(Vector2 source);
    Vector2 ToGlobal(Vector2 pos);
}
