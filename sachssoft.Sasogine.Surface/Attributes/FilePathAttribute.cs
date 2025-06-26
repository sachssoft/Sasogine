using System;
using sachssoft.Sasogine.Surface.Visuals.Controls;

namespace sachssoft.Sasogine.Surface.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class FilePathAttribute : Attribute
{
    public FileDialogMode DialogMode { get; private set; }
    public string Filter { get; private set; }
    public bool ShowPath { get; private set; }

    public FilePathAttribute(FileDialogMode dialogMode, string filter = "", bool showPath = false)
    {
        DialogMode = dialogMode;
        Filter = filter;
        ShowPath = showPath;
    }
}
