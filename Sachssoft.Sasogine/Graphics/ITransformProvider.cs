using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Graphics;

public interface ITransformProvider
{
    Matrix Projection { get; set; }

    Matrix View { get; set; }

    Matrix World { get; set; }
}
