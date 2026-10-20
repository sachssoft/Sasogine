using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Text;

namespace Sachssoft.Engine.Controls.Benchmarks;
/// <summary>Writes rendering-only raw samples, medians, and reproducibility information.</summary>
internal static class BenchmarkReport
{
    /// <summary>Exports the completed rendering comparison.</summary>
    public static void Write(BenchmarkOptions options, IReadOnlyList<Measurement> results, string adapter)
    {
        var csv = new StringBuilder("Library,Buttons,Round,Frames,CPU_draw_ms_per_frame\n");
        foreach (var r in results) csv.AppendLine(FormattableString.Invariant($"{r.Library},{r.Buttons},{r.Round},{r.Frames},{r.Milliseconds:F6}"));
        File.WriteAllText(Path.Combine(options.OutputPath, "raw.csv"), csv.ToString());
        var summary = new StringBuilder("Buttons,Sasogine_ms,Sasogine_min_ms,Sasogine_max_ms,Myra_ms,Myra_min_ms,Myra_max_ms,Median_winner,Difference_ms,Less_time_percent,Ranges_overlap\n");
        var md = new StringBuilder("# Sasogine / Myra: nur Rendering\n\n");
        md.AppendLine($"Gemessen: {DateTimeOffset.Now:O}. {RuntimeInformation.FrameworkDescription}; {RuntimeInformation.OSDescription}; {RuntimeInformation.ProcessArchitecture}.");
        md.AppendLine($"\nCPU: {Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER")}; {Environment.ProcessorCount} logische Prozessoren. Grafik: {adapter}.");
        md.AppendLine("\n| Buttons | Sasogine ms | Myra ms | Median-Sieger | Abstand ms | Weniger Zeit | Bereiche überlappen |\n|---:|---:|---:|---|---:|---:|---|");
        foreach (int count in options.Counts)
        {
            var c = new RenderComparison(count, results);
            string row = FormattableString.Invariant($"| {count} | {c.SasogineMs:F3} | {c.MyraMs:F3} | {c.Winner} | {c.Difference:F3} | {c.Percent:F1}% | {(c.Overlap ? "ja" : "nein")} |");
            md.AppendLine(row); Console.WriteLine(row);
            summary.AppendLine(FormattableString.Invariant($"{count},{c.SasogineMs:F6},{c.Sasogine[0]:F6},{c.Sasogine[^1]:F6},{c.MyraMs:F6},{c.Myra[0]:F6},{c.Myra[^1]:F6},{c.Winner},{c.Difference:F6},{c.Percent:F3},{c.Overlap}"));
        }
        md.AppendLine($"\n{options.Rounds} abwechselnde AB/BA-Runden, {options.Frames} Draws pro Batch; {options.Warmup} Warmup-Draws pro Bibliothek und Baumgröße. Angezeigt wird der Median der Batch-Mittelwerte. Prozent = (langsamere Zeit − kürzere Zeit) / langsamere Zeit. Überlappende Min/Max-Bereiche erlauben keinen belastbaren eindeutigen Sieger. Rohwerte: raw.csv; Min/Max: summary.csv.");
        md.AppendLine("\nGemessen werden ausschließlich `Root.Draw` und `Desktop.RenderVisual`: CPU-Wandzeit einschließlich möglicher Treiberwartezeiten. Keine GPU-Zeit und keine Spiel-FPS. Aufbau, Layout, Eingaben, Clear, RenderTarget-Wechsel, Present, Ergebnisanzeige und Pixel-Readback liegen außerhalb der Zeitmessung. Vor und nach JEDEM Draw synchronisiert ein Pixel-Readback den Grafiktreiber; dadurch wird Warteschlangenaufbau begrenzt. Das ist ein isolierter Draw-Vergleich, keine Simulation eines kompletten Spieleframes.");
        md.AppendLine("\nGleicher Prozess, GraphicsDevice und 1200 × 800 RenderTarget; VSync aus. Sichtbare Buttons mit identischer Geometrie, Farbe und je einem Textinhalt in gleichen StackPanel-Ebenen. Geometrie wird geprüft; PNGs dokumentieren beide Bäume. Beide verwenden dieselbe FontStashSharp-Fontinstanz in Größe 10. Sasogine wird aus aktuellen Controls-Quellen optimiert gebaut; Myra 1.6.8, MonoGame DesktopGL 3.8.6, FontStashSharp 1.6.2. Keine internen Rendereränderungen. Keine Aussage über komplexe Anwendungen oder andere Hardware; Hintergrundlast und thermische Effekte bleiben möglich.");
        md.AppendLine($"\nTieredCompilation: {Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") ?? "runtime default"}. Schrift SHA-256: `{Hash(options.FontPath)}`.\n\nControls SHA-256: `{Hash(Path.Combine(AppContext.BaseDirectory, "Sachssoft.Sasogine.Controls.dll"))}`.\n\nMyra SHA-256: `{Hash(Path.Combine(AppContext.BaseDirectory, "Myra.dll"))}`.");
        File.WriteAllText(Path.Combine(options.OutputPath, "summary.csv"), summary.ToString());
        File.WriteAllText(Path.Combine(options.OutputPath, "Vergleich.md"), md.ToString());
    }
    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
}
