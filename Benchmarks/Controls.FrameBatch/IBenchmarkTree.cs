namespace Sachssoft.Engine.Controls.Benchmarks;
/// <summary>Adapts a reusable UI tree to a real update, draw, and present loop.</summary>
internal interface IBenchmarkTree : IDisposable
{
    /// <summary>Gets the library name.</summary>
    string Library { get; }

    /// <summary>Prepares layout before warmup.</summary>
    void Prepare();
    /// <summary>Processes the UI update, unless bundled into Draw.</summary>
    void Update();
    /// <summary>Draws the UI; Myra also performs its bundled update here.</summary>
    void Draw();
    /// <summary>Verifies the expected visible geometry.</summary>
    void Verify();
}
