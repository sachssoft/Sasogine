namespace Sachssoft.Engine.Controls.Benchmarks;
/// <summary>Summarizes batch medians without implying statistical significance.</summary>
internal sealed class RenderComparison
{
    /// <summary>Builds a comparison from the currently available batches.</summary>
    public RenderComparison(int buttons, IReadOnlyList<Measurement> samples)
    {
        Buttons = buttons;
        Sasogine = samples.Where(x => x.Buttons == buttons && x.Library == "Sasogine").Select(x => x.Milliseconds).Order().ToArray();
        Myra = samples.Where(x => x.Buttons == buttons && x.Library == "Myra").Select(x => x.Milliseconds).Order().ToArray();
    }
    /// <summary>Gets the button count.</summary>
    public int Buttons { get; }
    /// <summary>Gets sorted Sasogine batch means.</summary>
    public double[] Sasogine { get; }
    /// <summary>Gets sorted Myra batch means.</summary>
    public double[] Myra { get; }
    /// <summary>Gets whether both implementations have samples.</summary>
    public bool Ready => Sasogine.Length > 0 && Myra.Length > 0;
    /// <summary>Gets the Sasogine median in milliseconds.</summary>
    public double SasogineMs => Median(Sasogine);
    /// <summary>Gets the Myra median in milliseconds.</summary>
    public double MyraMs => Median(Myra);
    /// <summary>Gets the implementation with the lower median.</summary>
    public string Winner => SasogineMs == MyraMs ? "Tie" : SasogineMs < MyraMs ? "Sasogine" : "Myra";
    /// <summary>Gets the absolute median difference in milliseconds.</summary>
    public double Difference => Math.Abs(SasogineMs - MyraMs);
    /// <summary>Gets the winner's reduction relative to the slower median, in percent.</summary>
    public double Percent => Difference / Math.Max(SasogineMs, MyraMs) * 100;
    /// <summary>Gets whether the observed batch ranges overlap.</summary>
    public bool Overlap => Sasogine[0] <= Myra[^1] && Myra[0] <= Sasogine[^1];
    private static double Median(double[] a) => a.Length == 0 ? double.NaN : (a[(a.Length - 1) / 2] + a[a.Length / 2]) / 2;
}
