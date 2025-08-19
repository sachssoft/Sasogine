using System.Collections.Generic;
using System.Linq;
using System;
using Sachssoft.Sasogine.Surface.Visuals.Controls;
using Sachssoft.Sasogine.Views;
using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Visuals;

namespace Sachssoft.Sasogine.Surface.Forms;

public static class FilePickerDialog
{

    public static void SelectFile(ViewBase view, string title, string path, IEnumerable<string> filter, Action<string> result)
    {
        var dlg = new FileDialog(FileDialogMode.OpenFile)
        {
            Title = title,
            Folder = path,
            Filter = string.Join(";", filter.Select(x => $"*." + x)),
            Background = Color.Red.ToBrush()
        };

        dlg.ButtonConfirm.Width = 100;
        dlg.ButtonConfirm.Content.HorizontalAlignment = HorizontalAlignment.Center;
        dlg.ButtonCancel.Width = 100;
        dlg.ButtonCancel.Content.HorizontalAlignment = HorizontalAlignment.Center;

        dlg.Closed += (s, e) =>
        {
            if (s is FileDialog dlg && dlg.Result == true)
            {
                result.Invoke(dlg.FilePath);
            }
        };

        dlg.ShowModal((Desktop)view.Host);
    }

    public static void SelectFolder(ViewBase view, string title, string path, Action<string> result)
    {
        var dlg = new FileDialog(FileDialogMode.ChooseFolder)
        {
            Title = title,
            Folder = path
        };

        dlg.ButtonOk.Width = 100;
        dlg.ButtonOk.Content.HorizontalAlignment = HorizontalAlignment.Center;
        dlg.ButtonCancel.Width = 100;
        dlg.ButtonCancel.Content.HorizontalAlignment = HorizontalAlignment.Center;

        dlg.Closed += (s, e) =>
        {
            if (s is FileDialog dlg && dlg.Result == true)
            {
                result.Invoke(dlg.FilePath);
            }
        };

        dlg.ShowModal((Desktop)view.Host);
    }

    public static void SaveFile(ViewBase view, string title, string path, IEnumerable<string> filter, Action<string> result)
    {
        var dlg = new FileDialog(FileDialogMode.SaveFile)
        {
            Title = title,
            Folder = path,
            Filter = string.Join(",", filter.Select(x => $"*." + x))
        };

        dlg.ButtonConfirm.Width = 100;
        dlg.ButtonConfirm.Content.HorizontalAlignment = HorizontalAlignment.Center;
        dlg.ButtonCancel.Width = 100;
        dlg.ButtonCancel.Content.HorizontalAlignment = HorizontalAlignment.Center;

        dlg.Closed += (s, e) =>
        {
            if (s is FileDialog dlg && dlg.Result == true)
            {
                result.Invoke(dlg.FilePath);
            }
        };

        dlg.ShowModal((Desktop)view.Host);
    }
}
