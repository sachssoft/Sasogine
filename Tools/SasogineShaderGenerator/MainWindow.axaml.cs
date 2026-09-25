using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Diagnostics;
using System.Text;

namespace SasogineShaderGenerator;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void BrowseFx_Click(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select MonoGame FX shader",
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("MonoGame Effect") { Patterns = ["*.fx"] }]
        });

        if (files.Count == 0)
            return;

        FxPathBox.Text = files[0].TryGetLocalPath();

        if (string.IsNullOrWhiteSpace(OutputPathBox.Text) && !string.IsNullOrWhiteSpace(FxPathBox.Text))
            OutputPathBox.Text = Path.ChangeExtension(FxPathBox.Text, ".mgfx");
    }

    private async void BrowseOutput_Click(object? sender, RoutedEventArgs e)
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Select output file",
            SuggestedFileName = GetSuggestedOutputName(),
            DefaultExtension = "mgfx",
            FileTypeChoices = [new FilePickerFileType("MonoGame Effect Binary") { Patterns = ["*.mgfx"] }]
        });

        if (file is not null)
            OutputPathBox.Text = file.TryGetLocalPath();
    }

    private async void Compile_Click(object? sender, RoutedEventArgs e)
    {
        var input = FxPathBox.Text?.Trim();
        var output = OutputPathBox.Text?.Trim();
        var compiler = CompilerPathBox.Text?.Trim();

        if (string.IsNullOrWhiteSpace(input) || !File.Exists(input))
        {
            SetLog("FX file does not exist.");
            return;
        }

        if (string.IsNullOrWhiteSpace(output))
        {
            SetLog("Output path is empty.");
            return;
        }

        if (string.IsNullOrWhiteSpace(compiler))
        {
            SetLog("mgfxc path is empty.");
            return;
        }

        var target = TargetBox.SelectedIndex == 1 ? "DirectX_11" : "OpenGL";
        var outputDirectory = Path.GetDirectoryName(output);
        if (!string.IsNullOrWhiteSpace(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        CompileButton.IsEnabled = false;
        SetLog($"Compiling {Path.GetFileName(input)} for {target}...{Environment.NewLine}");

        try
        {
            var result = await MgfxCompiler.CompileAsync(compiler, input, output, target);
            var log = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(result.StandardOutput))
                log.AppendLine(result.StandardOutput.TrimEnd());
            if (!string.IsNullOrWhiteSpace(result.StandardError))
                log.AppendLine(result.StandardError.TrimEnd());
            log.AppendLine(result.ExitCode == 0 ? $"Success: {output}" : $"Failed with exit code {result.ExitCode}.");
            SetLog(log.ToString());
        }
        catch (Exception ex)
        {
            SetLog(ex.ToString());
        }
        finally
        {
            CompileButton.IsEnabled = true;
        }
    }

    private string GetSuggestedOutputName()
    {
        var input = FxPathBox.Text;
        return string.IsNullOrWhiteSpace(input) ? "Shader.mgfx" : Path.GetFileNameWithoutExtension(input) + ".mgfx";
    }

    private void SetLog(string text) => LogBox.Text = text;
}

internal static class MgfxCompiler
{
    public static async Task<CompileResult> CompileAsync(string compilerPath, string inputPath, string outputPath, string profile, CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = compilerPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add(inputPath);
        startInfo.ArgumentList.Add(outputPath);
        startInfo.ArgumentList.Add($"/Profile:{profile}");

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        return new CompileResult(process.ExitCode, await stdoutTask, await stderrTask);
    }
}

internal readonly record struct CompileResult(int ExitCode, string StandardOutput, string StandardError);
