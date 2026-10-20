using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Sachssoft.Engine.Controls.Styling;
/// <summary>A validated atlas region and its explicit scaling mode.</summary>
public sealed record SkinRegion(Rectangle Source, string Mode, PixelInsets2 Slice);
