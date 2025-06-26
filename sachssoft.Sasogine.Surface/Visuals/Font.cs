//using FontStashSharp;
//using Microsoft.Xna.Framework.Graphics;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace sachssoft.Sasogine.Surface.Visuals;

//public sealed class Font
//{
//    private FontSystem _system;
//    private float _size;

//    // Default Font!
//    public Font()
//    {
//        var font = (DynamicSpriteFont)DefaultAssets.DefaultStylesheet.Fonts.FirstOrDefault().Value;
//        _system = font.FontSystem;
//    }

//    private Font(FontSystem system)
//    {
//        _system = system;
//    }

//    public static Font FromStream(System.IO.Stream stream)
//    {
//        var system = new FontSystem();
//        system.AddFont(stream);
//        return new Font(system);
//    }

//    public float Size
//    {
//        get => _size;
//        set => _size = value;
//    }

//    public int VirtualWidth
//    {
//        get; set;
//    }

//    public int VirtualHeight
//    {
//        get; set;
//    }

//    public SpriteFontBase ToFont()
//    {
//        float scale = UIEnvironment.GraphicsDevice.Viewport.Width / (float)VirtualWidth;

//        return _system.GetFont();
//    }
//}
