using Microsoft.Xna.Framework;

namespace sachssoft.Sasogine.Graphics;

public interface ITransformProvider
{
    Matrix Projection { get; set; }

    Matrix View { get; set; }

    Matrix World { get; set; }
}
