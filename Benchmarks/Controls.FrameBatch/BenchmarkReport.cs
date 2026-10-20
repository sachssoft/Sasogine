using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Sachssoft.Engine.Controls.Benchmarks;
/// <summary>Writes a controlled comparison of both Sasogine renderers and unmodified Myra.</summary>
internal static class BenchmarkReport
{
    private static readonly string[] Libraries = ["Sasogine SpriteBatch", "Sasogine FrameBatch", "Myra"];
    /// <summary>Gets the median of a nonempty sample sequence.</summary>
    public static double Median(IEnumerable<double> values)
    {
        double[] a = values.Order().ToArray();
        return (a[(a.Length - 1) / 2] + a[a.Length / 2]) / 2;
    }

    /// <summary>Computes per-library medians for one tree size.</summary>
    public static Measurement[] Summaries(IReadOnlyList<Measurement> samples, int count)
    {
        return Libraries.Select(lib =>
        {
            var rows = samples.Where(x => x.Buttons == count && x.Library == lib).ToArray();
            return new Measurement(lib, count, 0, rows.Sum(x => x.Frames), Median(rows.Select(x => x.Milliseconds)), Median(rows.Select(x => x.UiMilliseconds)), Median(rows.Select(x => x.DrawCalls)), Median(rows.Select(x => x.TextureSwitches)), Median(rows.Select(x => x.ShaderSwitches)));
        }).ToArray();
    }

    /// <summary>Exports raw batches, median summaries, and reproducibility limits.</summary>
    public static void Write(BenchmarkOptions options, IReadOnlyList<Measurement> samples, string adapter)
    {
        var csv = new StringBuilder("Library,Buttons,Round,Frames,Frame_ms,UI_Update_Draw_ms,Draw_calls,Texture_switches,Vertex_shader_switches\n");
        foreach (var s in samples)
            csv.AppendLine(FormattableString.Invariant($"{s.Library},{s.Buttons},{s.Round},{s.Frames},{s.Milliseconds:F6},{s.UiMilliseconds:F6},{s.DrawCalls:F3},{s.TextureSwitches:F3},{s.ShaderSwitches:F3}"));
        File.WriteAllText(Path.Combine(options.OutputPath, "raw.csv"), csv.ToString());
        var summary = new StringBuilder("Buttons,SpriteBatch_frame_ms,FrameBatch_frame_ms,Myra_frame_ms,FrameBatch_less_time_percent,SpriteBatch_draw_calls,FrameBatch_draw_calls,Myra_draw_calls,Median_winner\n");
        var md = new StringBuilder("# Sasogine FrameBatch / SpriteBatch / Myra\n\n");
        md.AppendLine($"Gemessen: {DateTimeOffset.Now:O}; {RuntimeInformation.FrameworkDescription}; {RuntimeInformation.OSDescription}; {adapter}; CPU: {Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER")}; {Environment.ProcessorCount} logische Prozessoren.");
        md.AppendLine("\n| Buttons | Sasogine SpriteBatch ms | Sasogine FrameBatch ms | Myra ms | FrameBatch weniger Zeit als SpriteBatch | Draw-Calls S/F/M | Median-Sieger |\n|---:|---:|---:|---:|---:|---|---|");
        foreach (int count in options.Counts)
        {
            var r = Summaries(samples, count);
            double saved = (r[0].Milliseconds - r[1].Milliseconds) / r[0].Milliseconds * 100;
            string winner = r.MinBy(x => x.Milliseconds)!.Library;
            string row = FormattableString.Invariant($"| {count} | {r[0].Milliseconds:F3} | {r[1].Milliseconds:F3} | {r[2].Milliseconds:F3} | {saved:F1}% | {r[0].DrawCalls:F0}/{r[1].DrawCalls:F0}/{r[2].DrawCalls:F0} | {winner} |");
            md.AppendLine(row);
            Console.WriteLine(row);
            summary.AppendLine(FormattableString.Invariant($"{count},{r[0].Milliseconds:F6},{r[1].Milliseconds:F6},{r[2].Milliseconds:F6},{saved:F3},{r[0].DrawCalls:F3},{r[1].DrawCalls:F3},{r[2].DrawCalls:F3},{winner}"));
        }

        md.AppendLine($"\n{options.Rounds} wechselnde Runden, je {options.Frames} gemessene echte Frames und {options.Warmup} Warmup-Frames pro Bibliotheksblock. Standardmäßig alle sechs Bibliotheksreihenfolgen. Median der Batch-Mittelwerte; Rohdaten einschließlich UI-Aufrufzeiten und Zustandszählern in raw.csv. Wiederholungsläufe zum Prüfen der Stabilität verwenden.");
        md.AppendLine("\n## Kontrollierter Vergleich\n\nBeide Sasogine-Bäume verwenden dieselben aktuellen Control-Quellen und denselben Layout-/Text-Cache. Nur ControlRenderer ist unterschiedlich. Beide Fontpfade verwenden dieselbe FontStashSharp-Fontinstanz; FrameBatch erhält native Glyph-Quads. Myra 1.6.8 wird unverändert verwendet. MonoGame DesktopGL 3.8.6, FontStashSharp 1.6.2. Geometrie und Anzahl der Controls werden vor und nach den Messungen geprüft.");
        md.AppendLine("\nEine reale Update-/Draw-/Present-Schleife direkt auf dem 1200 × 800 Backbuffer. VSync aus, variabler Timestep, InactiveSleepTime=0. Kein Readback, kein GPU-Drain, kein RenderTarget-Wechsel und keine Ergebnisanzeige während der Messung. Aufbau, Warmup und Ausgabe liegen außerhalb. Framezeit umfasst die Ereignisschleife zwischen Frames und normales Present. UI-Aufrufzeit umfasst Root.Update + Root.Draw beziehungsweise Myras vollständiges Desktop.Render. Das sind CPU-Wandzeiten einschließlich möglicher Treiberwartezeiten, keine isolierten GPU-Zeiten.");
        md.AppendLine("\n## Was FrameBatch ändert\n\nWiederverwendete Vertex-/Indexpuffer; CPU-Transformation und Clipping; keine SpriteBatch-Neustarts pro Control. Texture-Gruppen dürfen nur an späteren Gruppen vorbeiziehen, wenn ihre konservativen Rechtecke disjunkt sind. Überlappungen, Transparenz und explizite Kompatibilitätsbarrieren behalten ihre Reihenfolge. Buffergrenzen erzwingen geordnete Flushes. Layout und Zeichendaten der Controls werden nicht verändert. Der alte SpriteBatch-Pfad bleibt für Vergleich und bestehende Custom-Fonts/-Controls erreichbar.");
        md.AppendLine("\n## Geltungsbereich\n\nUnveränderte UI mit 100, 1.000 oder 5.000 sichtbaren Buttons, je einem Textinhalt und gleichen StackPanel-Ebenen. Einfarbige Hintergründe, identische Schrift und Größe 10; keine Spiellogik, Atlas-Skins, Animationen oder aktive Eingaben im Performance-Szenario. Maus synthetisch außerhalb, Keyboard leer. Darstellung mit Atlas-Skin, Clipping, Transparenz, DPI und Kompatibilitätsbarrieren wird separat geprüft. Ergebnisse hängen von Hardware, Treiber, Energiezustand und Hintergrundlast ab und sind keine allgemeine Geschwindigkeitsgarantie für komplette Spiele.");
        md.AppendLine($"\nTieredCompilation={Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") ?? "default"}.\n\nControls SHA-256: `{Hash(Path.Combine(AppContext.BaseDirectory, "Sachssoft.Sasogine.Controls.dll"))}`.\n\nMyra SHA-256: `{Hash(Path.Combine(AppContext.BaseDirectory, "Myra.dll"))}`.\n\nFont SHA-256: `{Hash(options.FontPath)}`.");
        File.WriteAllText(Path.Combine(options.OutputPath, "summary.csv"), summary.ToString());
        File.WriteAllText(Path.Combine(options.OutputPath, "Vergleich.md"), md.ToString());
        Console.WriteLine($"Results: {options.OutputPath}");
    }

    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
}
