namespace Sachssoft.Engine.Controls.Benchmarks;
/// <summary>Stores a block of consecutive real game frames, excluding setup and warmup.</summary>
internal sealed record Measurement(string Library, int Buttons, int Round, int Frames, double Milliseconds, double UiMilliseconds, double DrawCalls, double TextureSwitches, double ShaderSwitches);
