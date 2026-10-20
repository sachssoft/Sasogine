using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Sachssoft.Engine.Controls.Benchmarks;
/// <summary>Exports steady-state game-loop timings and graphics API counters.</summary>
internal static class BenchmarkReport
{
    /// <summary>Gets the median of nonempty values.</summary>
    public static double Median(IEnumerable<double> values)
    {
        double[] a=values.Order().ToArray();
        return (a[(a.Length-1)/2]+a[a.Length/2])/2;
    }
    /// <summary>Writes raw measured blocks and the comparison with explicit scope limits.</summary>
    public static void Write(BenchmarkOptions options,IReadOnlyList<Measurement> samples,string adapter)
    {
        var csv=new StringBuilder("Library,Buttons,Round,Frames,Frame_ms,UI_Update_Draw_ms,Draw_calls,Texture_switches,Vertex_shader_switches\n");
        foreach(var s in samples) csv.AppendLine(FormattableString.Invariant($"{s.Library},{s.Buttons},{s.Round},{s.Frames},{s.Milliseconds:F6},{s.UiMilliseconds:F6},{s.DrawCalls:F3},{s.TextureSwitches:F3},{s.ShaderSwitches:F3}"));
        File.WriteAllText(Path.Combine(options.OutputPath,"raw.csv"),csv.ToString());
        var summary=new StringBuilder("Buttons,Sasogine_frame_ms,Myra_frame_ms,Median_winner,Less_time_percent,Sasogine_UI_ms,Myra_UI_ms,Sasogine_draw_calls,Myra_draw_calls,Sasogine_min_ms,Sasogine_max_ms,Myra_min_ms,Myra_max_ms,Ranges_overlap\n");
        var md=new StringBuilder("# Sasogine / Myra: laufende Spielschleife\n\n");
        md.AppendLine($"Gemessen: {DateTimeOffset.Now:O}; {RuntimeInformation.FrameworkDescription}; {RuntimeInformation.OSDescription}; {adapter}; CPU: {Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER")}; {Environment.ProcessorCount} logische Prozessoren.");
        md.AppendLine("\n| Buttons | Sasogine Frame ms | Myra Frame ms | Median-Sieger | Weniger Zeit | UI CPU ms S/M | Draw-Calls S/M | Bereiche überlappen |\n|---:|---:|---:|---|---:|---|---|---|");
        foreach(int count in options.Counts)
        {
            var c=new RenderComparison(count,samples);
            var s=samples.Where(x=>x.Buttons==count && x.Library=="Sasogine").ToArray();
            var m=samples.Where(x=>x.Buttons==count && x.Library=="Myra").ToArray();
            double su=Median(s.Select(x=>x.UiMilliseconds)),mu=Median(m.Select(x=>x.UiMilliseconds));
            double sd=Median(s.Select(x=>x.DrawCalls)),mdraw=Median(m.Select(x=>x.DrawCalls));
            string row=FormattableString.Invariant($"| {count} | {c.SasogineMs:F3} | {c.MyraMs:F3} | {c.Winner} | {c.Percent:F1}% | {su:F3} / {mu:F3} | {sd:F0} / {mdraw:F0} | {(c.Overlap ? "ja" : "nein")} |");
            md.AppendLine(row); Console.WriteLine(row);
            summary.AppendLine(FormattableString.Invariant($"{count},{c.SasogineMs:F6},{c.MyraMs:F6},{c.Winner},{c.Percent:F3},{su:F6},{mu:F6},{sd:F3},{mdraw:F3},{c.Sasogine[0]:F6},{c.Sasogine[^1]:F6},{c.Myra[0]:F6},{c.Myra[^1]:F6},{c.Overlap}"));
        }
        md.AppendLine($"\n{options.Rounds} AB/BA-Runden, {options.Frames} gemessene echte Frames und {options.Warmup} ungemessene Warmup-Frames je Block. Median der Block-Mittelwerte; Prozent relativ zur langsameren Zeit. Überlappung ist ein Hinweis auf Schwankungen, kein statistischer Signifikanztest.");
        md.AppendLine("\n## Ablauf\n\nEin gemeinsamer Release-Prozess und ein GraphicsDevice. Pro Tick ein UI-Update und ein Draw auf den normalen 1200 × 800 Backbuffer, danach reguläres Present. Kein RenderTarget-Wechsel, keine Screenshots, kein GetData/Readback und kein GPU-Drain innerhalb der Messblöcke. VSync aus, variabler Timestep und InactiveSleepTime=0. Alle Controls und Fonts werden vor dem Block aufgebaut und aufgewärmt. AB/BA-Reihenfolge wechselt je Runde. Während der Messung zeichnet ausschließlich die aktive Bibliothek; Ergebnisanzeige und Konsolenausgabe finden außerhalb der Blöcke statt.");
        md.AppendLine("\nFramezeit = reale Wandzeit vom Beginn des ersten gemessenen Updates bis nach Present des letzten Frames, geteilt durch Framezahl; einschließlich Ereignisschleife zwischen den Frames, Clear und möglicher Treiberwartezeiten. UI CPU = ausschließlich Aufrufzeiten von Root.Update + Root.Draw beziehungsweise Desktop.Render. Myras öffentliche Render-Methode enthält Layout, Input, Ereignisse und Zeichnen; keine künstliche Aufteilung oder Subtraktion. Draw-Calls und Zustandszähler werden direkt um den jeweiligen UI-Aufruf über GraphicsDevice.Metrics erfasst. Das sind API-Aufrufe, keine GPU-Zeitstempel. Present kann GPU-Arbeit verzögert abarbeiten; keine isolierte GPU-Zeit.");
        md.AppendLine("\n## Geltungsbereich\n\nUnveränderte sichtbare UI: 100, 1.000 oder 5.000 Buttons mit je einem TextBlock/Label und zwei StackPanel-Ebenen. Identische Button-Geometrie und Elementzahl werden geprüft. Einfarbige Hintergründe, gleiche FontStashSharp-Fontinstanz (Segoe UI, Größe 10), keine Atlas-Skins, Klicks, Textänderungen oder Spiellogik. Maus steht synthetisch außerhalb; Keyboard leer. Sasogine nutzt den aktuellen Layout-/Text-Cache. Dies ist eine laufende UI-Spielschleife, kein vollständiges Spiel und keine allgemeine FPS-Aussage über Sasogine oder Myra. Betriebssystem, Treiber, Energiezustand und Hintergrundlast können Ergebnisse ändern. Frühere isolierte Draw-Messungen mit GPU-Readback haben eine andere Methodik und sind nicht direkt vergleichbar.");
        md.AppendLine("\n## SpriteBatch\n\nSasogine beendet und beginnt aktuell in ControlDrawContext.SetControl je Traversierungsabschnitt einen Batch; Root.DrawTree ruft dies zweimal pro sichtbarem Element auf. Das kann CPU-Verwaltung und Zustandswechsel erzeugen. Nicht jeder Begin/End-Aufruf verursacht einen GPU-Draw, etwa bei leeren Batches. Die gemessenen Draw-Call-Zahlen zeigen tatsächliche Grafik-API-Aufrufe, nicht die Anzahl der Begin/End-Paare. Gleiche Draw-Call-Zahlen beweisen keine gleichen CPU-Kosten. SpriteBatch ist grundsätzlich für gebündeltes 2D-Zeichnen ausgelegt; viele kleine Batches sind ein möglicher Engpass. Dessen genauer Zeitanteil wurde hier nicht durch eine Rendereränderung isoliert.");
        md.AppendLine($"\nMyra 1.6.8; MonoGame DesktopGL 3.8.6; FontStashSharp 1.6.2; .NET TieredCompilation={Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") ?? "default"}.\n\nControls SHA-256: `{Hash(Path.Combine(AppContext.BaseDirectory,"Sachssoft.Sasogine.Controls.dll"))}`.\n\nMyra SHA-256: `{Hash(Path.Combine(AppContext.BaseDirectory,"Myra.dll"))}`.\n\nFont SHA-256: `{Hash(options.FontPath)}`.");
        File.WriteAllText(Path.Combine(options.OutputPath,"summary.csv"),summary.ToString());
        File.WriteAllText(Path.Combine(options.OutputPath,"Vergleich.md"),md.ToString());
        Console.WriteLine($"Results: {options.OutputPath}");
    }
    private static string Hash(string path)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
}
