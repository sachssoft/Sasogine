using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace Sachssoft.TextureConverter;

/// <summary>
/// Provides a small graphical front end for image and MonoGame pixel conversion.
/// </summary>
public sealed partial class MainWindow : Window
{
    private static readonly string[] OutputExtensions = [".png", ".jpg", ".webp", ".bmp", ".tga", ".raw"];

    /// <summary>
    /// Initializes the converter window and its controls.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void BrowseInput_Click(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select an image",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Supported images")
                {
                    Patterns = ["*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif", "*.webp", "*.tga"]
                }
            ]
        });

        if (files.Count > 0)
            InputPathBox.Text = files[0].TryGetLocalPath();
    }

    private async void BrowseOutput_Click(object? sender, RoutedEventArgs e)
    {
        var extension = SelectedExtension;
        var path = OutputPathBox.Text?.Trim();
        var selected = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Select output file",
            SuggestedFileName = string.IsNullOrWhiteSpace(path) ? "texture" + extension : Path.GetFileName(path),
            DefaultExtension = extension.TrimStart('.'),
            ShowOverwritePrompt = ReplaceCheckBox.IsChecked == true,
            FileTypeChoices = [new FilePickerFileType(extension.ToUpperInvariant() + " file") { Patterns = ["*" + extension] }]
        });

        if (selected?.TryGetLocalPath() is { } output)
            OutputPathBox.Text = Path.ChangeExtension(output, extension);
    }

    private void InputPathBox_TextChanged(object? sender, TextChangedEventArgs e) => UpdateOutputPath();

    private void Format_SelectionChanged(object? sender, SelectionChangedEventArgs e) => UpdateOutputPath();

    private void ReplaceCheckBox_Changed(object? sender, RoutedEventArgs e) => UpdateOutputPath();

    private string SelectedExtension => OutputExtensions[Math.Clamp(FileFormatBox.SelectedIndex, 0, OutputExtensions.Length - 1)];

    private void UpdateOutputPath()
    {
        if (InputPathBox is null || OutputPathBox is null || PixelFormatBox is null || FileFormatBox is null || ReplaceCheckBox is null)
            return;

        var input = InputPathBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            OutputPathBox.Text = string.Empty;
            return;
        }

        var extension = SelectedExtension;
        var directory = Path.GetDirectoryName(input);
        var name = Path.GetFileNameWithoutExtension(input);
        var pixelFormat = (MonoGamePixelFormat)Math.Clamp(PixelFormatBox.SelectedIndex, 0, 5);
        var outputName = ReplaceCheckBox.IsChecked == true ? name + extension : $"{name}_{pixelFormat}{extension}";

        OutputPathBox.Text = string.IsNullOrWhiteSpace(directory)
            ? outputName
            : Path.Combine(directory, outputName);
    }

    private async void Convert_Click(object? sender, RoutedEventArgs e)
    {
        var input = InputPathBox.Text?.Trim();
        var output = OutputPathBox.Text?.Trim();

        if (string.IsNullOrWhiteSpace(input) || !File.Exists(input))
        {
            SetLog("Input image does not exist.");
            return;
        }

        if (string.IsNullOrWhiteSpace(output))
        {
            SetLog("Output path is empty.");
            return;
        }

        var format = (MonoGamePixelFormat)PixelFormatBox.SelectedIndex;
        var extension = SelectedExtension;
        if (!Path.GetExtension(output).Equals(extension, StringComparison.OrdinalIgnoreCase))
        {
            SetLog($"Output filename must end in {extension}.");
            return;
        }

        var replace = ReplaceCheckBox.IsChecked == true;

        ConvertButton.IsEnabled = false;
        SetLog($"Converting {Path.GetFileName(input)} to {format} / {extension} ...");

        try
        {
            var result = await Task.Run(() => ImageConverter.Convert(input, output, format, replace));
            SetLog(result);
        }
        catch (Exception ex)
        {
            SetLog($"Conversion failed:{Environment.NewLine}{ex}");
        }
        finally
        {
            ConvertButton.IsEnabled = true;
        }
    }

    private void SetLog(string message) => LogBox.Text = message;
}
