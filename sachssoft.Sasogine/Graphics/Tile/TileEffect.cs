using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Graphics.Tile;

public class TileEffect : Effect
{
    private readonly EffectParameter _worldViewProjectionParam;
    private readonly EffectParameter _textureParam;

    public TileEffect(GraphicsDevice device, byte[] effectBytecode) : base(device, effectBytecode)
    {
        _worldViewProjectionParam = Parameters["WorldViewProjection"];
        _textureParam = Parameters["Texture"];
    }

    public Matrix WorldViewProjection
    {
        get => _worldViewProjectionParam.GetValueMatrix();
        set => _worldViewProjectionParam.SetValue(value);
    }

    public Texture2D Texture
    {
        get => _textureParam.GetValueTexture2D();
        set => _textureParam.SetValue(value);
    }
}