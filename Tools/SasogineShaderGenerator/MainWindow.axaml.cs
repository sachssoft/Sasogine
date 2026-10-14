
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        UpdateOutputPath();
    }

    private void FxPathBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateOutputPath();
    }

    private void TargetBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateOutputPath();
    }

    private void TargetSubfolderBox_Changed(object? sender, RoutedEventArgs e)
    {
        UpdateOutputPath();
    }

    private void UpdateOutputPath()
    {
        if (OutputPathBox is null || FxPathBox is null || TargetBox is null)
            return;

        var input = FxPathBox.Text?.Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            OutputPathBox.Text = string.Empty;
            return;
        }

        var directory = Path.GetDirectoryName(input);
        var fileName = Path.GetFileNameWithoutExtension(input);
        var suffix = GetTarget().Suffix;
        var outputName = $"{fileName}_{suffix}.mgfxo";

        if (TargetSubfolderBox?.IsChecked == true)
            directory = string.IsNullOrWhiteSpace(directory) ? suffix : Path.Combine(directory, suffix);

        OutputPathBox.Text = string.IsNullOrWhiteSpace(directory)
            ? outputName
            : Path.Combine(directory, outputName);
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
            SetLog("MGFXC compiler path is empty.");
            return;
        }

        if (string.Equals(compiler, "mgfxo", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(compiler, "mgfxo.exe", StringComparison.OrdinalIgnoreCase))
        {
            compiler = "mgfxc";
            CompilerPathBox.Text = compiler;
        }

        var target = GetTarget().Profile;

        CompileButton.IsEnabled = false;
        SetLog($"Compiling {Path.GetFileName(input)} for {target}...{Environment.NewLine}");

        try
        {
            var outputDirectory = Path.GetDirectoryName(Path.GetFullPath(output));

            if (!string.IsNullOrWhiteSpace(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            var existed = File.Exists(output);

            var result = await MgfxCompiler.CompileAsync(compiler, input, output, target);

            var log = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(result.StandardOutput))
                log.AppendLine(result.StandardOutput.TrimEnd());

            if (!string.IsNullOrWhiteSpace(result.StandardError))
                log.AppendLine(result.StandardError.TrimEnd());

            if (result.ExitCode == 0)
            {
                log.AppendLine(existed
                    ? $"Successfully replaced: {output}"
                    : $"Successfully created: {output}");
            }
            else
            {
                log.AppendLine($"Failed with exit code {result.ExitCode}.");
                log.AppendLine("Existing output file was preserved.");
            }

            SetLog(log.ToString());
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode is 2 or 3)
        {
            SetLog(
                $"MGFXC compiler not found: {compiler}{Environment.NewLine}" +
                $"Install MonoGame MGFXC using:{Environment.NewLine}" +
                $"dotnet tool install --global dotnet-mgfxc --version 3.8.6{Environment.NewLine}{Environment.NewLine}" +
                $"Or enter the full path to mgfxc.exe in the 'mgfxc' field.");
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

    private (string Profile, string Suffix) GetTarget() => TargetBox.SelectedIndex switch
    {
        1 => ("DirectX_11", "dx"),
        2 => ("DirectX_12", "dx12"),
        3 => ("Vulkan", "vk"),
        _ => ("OpenGL", "gl")
    };

    private void SetLog(string text) => LogBox.Text = text;
}

internal static class MgfxCompiler
{
    public static async Task<CompileResult> CompileAsync(
        string compilerPath,
        string inputPath,
        string outputPath,
        string profile,
        CancellationToken cancellationToken = default)
    {
        var fullOutputPath = Path.GetFullPath(outputPath);
        var outputDirectory = Path.GetDirectoryName(fullOutputPath)!;

        Directory.CreateDirectory(outputDirectory);

        // Always compile into a temporary output file first.
        // The existing shader remains untouched if compilation fails.
        var temporaryOutputPath = Path.Combine(
            outputDirectory,
            $".{Path.GetFileNameWithoutExtension(fullOutputPath)}_{Guid.NewGuid():N}.mgfxo");

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = ResolveCompilerPath(compilerPath),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            startInfo.ArgumentList.Add(inputPath);
            startInfo.ArgumentList.Add(temporaryOutputPath);
            startInfo.ArgumentList.Add($"/Profile:{profile}");

            using var process = new Process { StartInfo = startInfo };

            process.Start();

            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            if (process.ExitCode == 0)
            {
                if (!File.Exists(temporaryOutputPath))
                {
                    throw new FileNotFoundException(
                        "MGFXC completed successfully but did not generate an output file.",
                        temporaryOutputPath);
                }

                // Replace the existing shader only after successful compilation.
                File.Move(temporaryOutputPath, fullOutputPath, overwrite: true);
            }

            return new CompileResult(process.ExitCode, stdout, stderr);
        }
        finally
        {
            // Remove temporary output on failure or after replacement.
            if (File.Exists(temporaryOutputPath))
                File.Delete(temporaryOutputPath);
        }
    }

    private static string ResolveCompilerPath(string compilerPath)
    {
        if (!string.Equals(compilerPath, "mgfxc", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(compilerPath, "mgfxc.exe", StringComparison.OrdinalIgnoreCase))
        {
            return compilerPath;
        }

        if (OperatingSystem.IsWindows())
        {
            var shim = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".dotnet",
                "tools",
                "mgfxc.exe");

            if (File.Exists(shim))
                return shim;
        }

        return compilerPath;
    }
}

internal readonly record struct CompileResult(
    int ExitCode,
    string StandardOutput,
    string StandardError);
