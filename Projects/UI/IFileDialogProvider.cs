using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// Represents ifile dialog provider.
/// </summary>
public interface IFileDialogProvider
{
    /// <summary>
    /// Reads a directory snapshot or throws an IO/access exception.
    /// </summary>
    FileDialogDirectory ReadDirectory(string path);
    /// <summary>
    /// Resolves a typed path against a directory.
    /// </summary>
    string Resolve(string directory, string name);
    /// <summary>
    /// Checks a file or directory before accepting a result.
    /// </summary>
    bool Exists(string path, bool directory);
}
