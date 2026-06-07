using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Resources
{
    public class NinePatchRegion : ITextureRegion
    {
        private readonly Rectangle[] _regions;

        // 1️⃣ Konstruktor: FullRect + Randgrößen
        public NinePatchRegion(Rectangle fullRect, int left, int right, int top, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;

            int centerWidth = fullRect.Width - left - right;
            int centerHeight = fullRect.Height - top - bottom;

            _regions = new Rectangle[9];

            // Ecken
            _regions[0] = new Rectangle(fullRect.X, fullRect.Y, left, top); // top-left
            _regions[2] = new Rectangle(fullRect.X + fullRect.Width - right, fullRect.Y, right, top); // top-right
            _regions[6] = new Rectangle(fullRect.X, fullRect.Y + fullRect.Height - bottom, left, bottom); // bottom-left
            _regions[8] = new Rectangle(fullRect.X + fullRect.Width - right, fullRect.Y + fullRect.Height - bottom, right, bottom); // bottom-right

            // Kanten
            _regions[1] = new Rectangle(fullRect.X + left, fullRect.Y, centerWidth, top); // top
            _regions[3] = new Rectangle(fullRect.X, fullRect.Y + top, left, centerHeight); // left
            _regions[5] = new Rectangle(fullRect.X + fullRect.Width - right, fullRect.Y + top, right, centerHeight); // right
            _regions[7] = new Rectangle(fullRect.X + left, fullRect.Y + fullRect.Height - bottom, centerWidth, bottom); // bottom

            // Mitte
            _regions[4] = new Rectangle(fullRect.X + left, fullRect.Y + top, centerWidth, centerHeight); // center
        }

        // 2️⃣ Konstruktor: direkt alle 9 Rechtecke
        public NinePatchRegion(Rectangle[] rects)
        {
            if (rects.Length != 9)
                throw new ArgumentException("NinePatchRegion requires exactly 9 rectangles.", nameof(rects));

            _regions = (Rectangle[])rects.Clone(); // optional, schützt vor externen Änderungen

            // Randgrößen ableiten
            Left = _regions[0].Width;
            Top = _regions[0].Height;
            Right = _regions[2].Width;
            Bottom = _regions[6].Height;
        }

        // IReadOnlyList für echte Immutability
        public IReadOnlyList<Rectangle> Regions => _regions;

        public int Left { get; }

        public int Top { get; }

        public int Right { get; }

        public int Bottom { get; }
    }
}