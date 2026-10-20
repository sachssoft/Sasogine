namespace Sachssoft.Engine.Controls.Benchmarks;
/// <summary>Provides a prepared UI tree through public library APIs.</summary>
internal interface IBenchmarkTree : IDisposable
{
    /// <summary>Gets the library name.</summary>
    string Library { get; }
    /// <summary>Completes layout before rendering is measured.</summary>
    void Prepare();
    /// <summary>Renders the prepared tree without processing input or layout.</summary>
    void Draw();
    /// <summary>Checks the visible button geometry.</summary>
    void Verify();
}
