using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace Sachssoft.SvgConverter;

/// <summary>Provides the graphical interface for batch SVG-to-PNG conversion.</summary>
public sealed partial class MainWindow : Window
{
    private readonly ObservableCollection<string> _files = [];

    /// <summary>Initializes the conversion window.</summary>
    public MainWindow()
    {
        InitializeComponent();
        FilesList.ItemsSource = _files;
    }

    private async void AddFiles_Click(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select SVG files", AllowMultiple = true,
            FileTypeFilter = [new FilePickerFileType("SVG graphics") { Patterns = ["*.svg"], MimeTypes = ["image/svg+xml"] }]
        });

        foreach (var file in files)
        {
            var path = file.TryGetLocalPath();
            if (path is not null && path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase) &&
                !_files.Contains(path, StringComparer.OrdinalIgnoreCase))
                _files.Add(path);
        }

        if (_files.Count > 0 && string.IsNullOrWhiteSpace(OutputPathBox.Text))
            OutputPathBox.Text = Path.GetDirectoryName(_files[0]);

        StatusText.Text = $"{_files.Count} SVG file(s) selected.";
    }

    private void RemoveSelected_Click(object? sender, RoutedEventArgs e)
    {
        foreach (var path in FilesList.SelectedItems?.OfType<string>().ToArray() ?? [])
            _files.Remove(path);
        StatusText.Text = $"{_files.Count} SVG file(s) selected.";
    }

    private void Clear_Click(object? sender, RoutedEventArgs e)
    {
        _files.Clear();
        StatusText.Text = "No files selected.";
    }

    private async void BrowseOutput_Click(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select PNG output folder", AllowMultiple = false
        });
        if (folders.Count > 0)
            OutputPathBox.Text = folders[0].TryGetLocalPath();
    }

    private void SizingChanged(object? sender, RoutedEventArgs e)
    {
        if (ScalePanel is null || TargetPanel is null || DimensionBox is null) return;
        var target = TargetModeBox.IsChecked == true;
        ScalePanel.IsVisible = !target;
        TargetPanel.IsVisible = target;
        var dimension = DimensionBox.SelectedIndex;
        WidthBox.IsEnabled = dimension != 2;
        HeightBox.IsEnabled = dimension != 1;
        KeepRatioBox.IsEnabled = dimension == 0;
    }

    private void DimensionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        SizingChanged(sender, e);
    }

    private void SuffixChanged(object? sender, RoutedEventArgs e)
    {
        if (SuffixTextBox is not null)
            SuffixTextBox.IsEnabled = SuffixBox?.IsChecked == true;
    }

    private async void Convert_Click(object? sender, RoutedEventArgs e)
    {
        var output = OutputPathBox.Text?.Trim();
        var scales = new List<int>(4);
        if (Scale1Box.IsChecked == true) scales.Add(1);
        if (Scale2Box.IsChecked == true) scales.Add(2);
        if (Scale3Box.IsChecked == true) scales.Add(3);
        if (Scale4Box.IsChecked == true) scales.Add(4);

        if (_files.Count == 0) { LogBox.Text = "Please add SVG files first."; return; }
        if (string.IsNullOrWhiteSpace(output)) { LogBox.Text = "Please select an output folder."; return; }
        var targetMode = TargetModeBox.IsChecked == true;
        if (!targetMode && scales.Count == 0) { LogBox.Text = "Select at least one scale factor."; return; }
        int? width = null, height = null;
        if (targetMode)
        {
            if (DimensionBox.SelectedIndex != 2)
            {
                if (!int.TryParse(WidthBox.Text, NumberStyles.None, CultureInfo.InvariantCulture, out int parsedWidth))
                { LogBox.Text = "Please enter a valid positive target width."; return; }
                width = parsedWidth;
            }
            if (DimensionBox.SelectedIndex != 1)
            {
                if (!int.TryParse(HeightBox.Text, NumberStyles.None, CultureInfo.InvariantCulture, out int parsedHeight))
                { LogBox.Text = "Please enter a valid positive target height."; return; }
                height = parsedHeight;
            }
            if (width is <= 0 or > 16384 || height is <= 0 or > 16384)
            { LogBox.Text = "Target dimensions must be between 1 and 16384 pixels."; return; }
        }

        var addSuffix = SuffixBox.IsChecked == true;
        var suffix = addSuffix ? SuffixTextBox.Text ?? "" : "";
        if (addSuffix && (string.IsNullOrWhiteSpace(suffix) ||
            suffix.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || suffix.Contains('/') || suffix.Contains('\\') ||
            suffix.EndsWith('.') || suffix.EndsWith(' ')))
        { LogBox.Text = "Please enter a valid filename suffix."; return; }

        var options = new ResizeOptions(targetMode ? ResizeMode.TargetSize : ResizeMode.Scale,
            scales, width, height, KeepRatioBox.IsChecked == true, addSuffix, suffix);

        var files = _files.ToArray();
        var overwrite = OverwriteBox.IsChecked == true;
        ConvertButton.IsEnabled = false;
        LogBox.Text = "Converting...";
        StatusText.Text = "Working...";

        try
        {
            var report = await Task.Run(() => SvgConverter.ConvertBatch(files, output, options, overwrite));
            LogBox.Text = report.Log;
            StatusText.Text = $"Created: {report.Created} · Skipped: {report.Skipped} · Errors: {report.Errors}";
        }
        catch (Exception ex)
        {
            LogBox.Text = $"Conversion failed: {ex.Message}";
            StatusText.Text = "Conversion failed.";
        }
        finally { ConvertButton.IsEnabled = true; }
    }
}
