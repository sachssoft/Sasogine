using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace Sachssoft.ImageResizer;

/// <summary>Provides the desktop interface for batch image resizing.</summary>
public sealed partial class MainWindow : Window
{
    /// <summary>Initializes the resizer window.</summary>
    public MainWindow() => InitializeComponent();

    private async void BrowseInput_Click(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Select image source folder", AllowMultiple = false });
        if (folders.Count > 0) InputPathBox.Text = folders[0].TryGetLocalPath();
    }

    private async void Convert_Click(object? sender, RoutedEventArgs e)
    {
        var input = InputPathBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(input) || !Directory.Exists(input)) { LogBox.Text = "Source folder does not exist."; return; }
        if (!int.TryParse(SizeBox.Text, out var size) || size < 1 || size > 32768) { LogBox.Text = "Target size must be between 1 and 32768 pixels."; return; }
        var suffix = SuffixBox.Text ?? "";
        if (suffix.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || suffix.Contains('/') || suffix.Contains('\\')) { LogBox.Text = "Invalid filename suffix."; return; }
        // Capture all Avalonia control values on the UI thread before starting background work.
        var byWidth = DimensionBox.SelectedIndex == 0;
        var trim = TrimBox.IsChecked == true;
        var recursive = RecursiveBox.IsChecked == true;
        ConvertButton.IsEnabled = false;
        LogBox.Text = "Processing images…";
        try
        {
            var result = await Task.Run(() => ImageProcessor.Process(input, size, byWidth, suffix, trim, recursive));
            LogBox.Text = result;
        }
        catch (Exception ex) { LogBox.Text = $"Failed: {ex.Message}"; }
        finally { ConvertButton.IsEnabled = true; }
    }
}
