namespace Sachssoft.Engine.Controls.Benchmarks;
/// <summary>Stores the average CPU draw time of one batch in milliseconds.</summary>
internal sealed record Measurement(string Library, int Buttons, int Round, int Frames, double Milliseconds);
