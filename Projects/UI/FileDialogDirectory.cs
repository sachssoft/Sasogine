using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// FileDialogDirectory configuration and behavior.
/// </summary>
public sealed record FileDialogDirectory(string FullPath, string? ParentPath, IReadOnlyList<FileDialogEntry> Entries);
