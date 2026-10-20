using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sachssoft.Engine.Controls;
/// <summary>A file-browser entry supplied by an explicit filesystem adapter.</summary>
public sealed record FileDialogEntry(string Name, string FullPath, bool IsDirectory);
