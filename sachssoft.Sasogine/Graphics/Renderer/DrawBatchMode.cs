namespace sachssoft.Sasogine.Graphics.Renderer;

/// <summary>
/// Defines how draw calls are batched and ordered during rendering.
/// </summary>
public enum DrawBatchMode
{
    /// <summary>
    /// Draw calls are issued immediately without sorting or batching.
    /// </summary>
    Immediate = 0,

    /// <summary>
    /// Draw calls are sorted from back to front, typically used for transparency.
    /// </summary>
    BackToFront = 1,

    /// <summary>
    /// Draw calls are sorted from front to back, typically used for performance optimizations with depth buffering.
    /// </summary>
    FrontToBack = 2
}