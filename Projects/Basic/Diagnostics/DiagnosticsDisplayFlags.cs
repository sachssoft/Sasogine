using System;

namespace Sachssoft.Sasogine.Diagnostics;

/// <summary>
/// Specifies the diagnostic information and overlays that can be displayed.
/// </summary>
[Flags]
public enum DiagnosticsDisplayFlags : ulong
{
    /// <summary>
    /// No diagnostic information is displayed.
    /// </summary>
    None = 0,

    /// <summary>
    /// Displays frame rate information.
    /// </summary>
    FPS = 1UL << 0,

    /// <summary>
    /// Displays the number of draw calls.
    /// </summary>
    DrawCalls = 1UL << 1,

    /// <summary>
    /// Displays memory usage information.
    /// </summary>
    MemoryUsage = 1UL << 2,

    /// <summary>
    /// Displays CPU usage information.
    /// </summary>
    CPUUsage = 1UL << 3,

    /// <summary>
    /// Displays GPU usage information.
    /// </summary>
    GPUUsage = 1UL << 4,

    /// <summary>
    /// Displays colliders and their bounds.
    /// </summary>
    Colliders = 1UL << 5,

    /// <summary>
    /// Displays active collision information.
    /// </summary>
    Collisions = 1UL << 6,

    /// <summary>
    /// Displays actor diagnostic information.
    /// </summary>
    Actors = 1UL << 7,

    /// <summary>
    /// Displays camera bounds and the visible area.
    /// </summary>
    CameraBounds = 1UL << 8,

    /// <summary>
    /// Displays the camera view frustum.
    /// </summary>
    Frustum = 1UL << 9,

    /// <summary>
    /// Displays current input information.
    /// </summary>
    Input = 1UL << 10,

    /// <summary>
    /// Displays input mapping information.
    /// </summary>
    InputMapping = 1UL << 11,

    /// <summary>
    /// Displays physics diagnostic information.
    /// </summary>
    Physics = 1UL << 12,

    /// <summary>
    /// Displays paths, waypoints, and routes.
    /// </summary>
    Paths = 1UL << 13,

    /// <summary>
    /// Displays the diagnostic grid.
    /// </summary>
    Grid = 1UL << 14,

    /// <summary>
    /// Displays grid cell information.
    /// </summary>
    GridCells = 1UL << 15,

    /// <summary>
    /// Displays regions and trigger areas.
    /// </summary>
    Regions = 1UL << 16,

    /// <summary>
    /// Displays general diagnostic text.
    /// </summary>
    DebugText = 1UL << 17,

    /// <summary>
    /// Displays particle system diagnostic information.
    /// </summary>
    Particles = 1UL << 18,

    /// <summary>
    /// Displays artificial intelligence diagnostic information.
    /// </summary>
    AI = 1UL << 19,

    /// <summary>
    /// Displays network and synchronization information.
    /// </summary>
    Network = 1UL << 20,

    /// <summary>
    /// Displays animation state and frame information.
    /// </summary>
    Animations = 1UL << 21,

    /// <summary>
    /// Displays visual effect diagnostic information.
    /// </summary>
    Effects = 1UL << 22,

    /// <summary>
    /// Displays audio diagnostic information.
    /// </summary>
    Audio = 1UL << 23,

    /// <summary>
    /// Displays layer and depth information.
    /// </summary>
    Layers = 1UL << 24,

    /// <summary>
    /// Displays shader diagnostic information.
    /// </summary>
    Shaders = 1UL << 25,

    /// <summary>
    /// Displays the performance graph.
    /// </summary>
    PerformanceGraph = 1UL << 26,

    /// <summary>
    /// Displays runtime timer information.
    /// </summary>
    Timers = 1UL << 27,

    /// <summary>
    /// Displays runtime log messages.
    /// </summary>
    LogMessages = 1UL << 28,

    /// <summary>
    /// Displays runtime event information.
    /// </summary>
    Events = 1UL << 29,

    /// <summary>
    /// Displays object and actor state information.
    /// </summary>
    States = 1UL << 30,

    /// <summary>
    /// Displays user interface diagnostic information.
    /// </summary>
    UI = 1UL << 31,

    /// <summary>
    /// Displays element diagnostic information.
    /// </summary>
    Elements = 1UL << 32,

    /// <summary>
    /// Displays editor diagnostic information.
    /// </summary>
    Editor = 1UL << 33
}