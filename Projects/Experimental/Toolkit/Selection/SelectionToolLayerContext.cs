using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Selection;

/// <summary>
/// Provides the settings and values required by selection tool layers
/// while updating interaction nodes and processing target interactions.
/// </summary>
public sealed class SelectionToolLayerContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectionToolLayerContext"/> class.
    /// </summary>
    /// <param name="enableGridSnap">
    /// Indicates whether grid-based snapping is enabled.
    /// </param>
    /// <param name="gridSnapStep">
    /// The grid step used for position- and size-based snapping.
    /// </param>
    /// <param name="enableAngleSnap">
    /// Indicates whether angle-based snapping is enabled.
    /// </param>
    /// <param name="angleSnapStep">
    /// The angle step in radians used for angle-based snapping.
    /// </param>
    /// <param name="enablePivotSnap">
    /// Indicates whether pivot snapping is enabled.
    /// </param>
    /// <param name="pivotSnapStep">
    /// The normalized step used for pivot snapping.
    /// </param>
    /// <param name="handleSize">
    /// The size of selection tool interaction handles.
    /// </param>
    public SelectionToolLayerContext(
        bool enableGridSnap,
        Size2 gridSnapStep,
        bool enableAngleSnap,
        float angleSnapStep,
        bool enablePivotSnap,
        Vector2 pivotSnapStep,
        float handleSize)
    {
        EnableGridSnap = enableGridSnap;
        GridSnapStep = gridSnapStep;
        EnableAngleSnap = enableAngleSnap;
        AngleSnapStep = angleSnapStep;
        EnablePivotSnap = enablePivotSnap;
        PivotSnapStep = pivotSnapStep;
        HandleSize = handleSize;
    }

    /// <summary>
    /// Gets a value indicating whether grid-based snapping is enabled.
    /// </summary>
    public bool EnableGridSnap { get; }

    /// <summary>
    /// Gets the grid step used for position- and size-based snapping.
    /// </summary>
    public Size2 GridSnapStep { get; }

    /// <summary>
    /// Gets a value indicating whether angle-based snapping is enabled.
    /// </summary>
    public bool EnableAngleSnap { get; }

    /// <summary>
    /// Gets the angle step in radians used for angle-based snapping.
    /// </summary>
    public float AngleSnapStep { get; }

    /// <summary>
    /// Gets a value indicating whether pivot snapping is enabled.
    /// </summary>
    public bool EnablePivotSnap { get; }

    /// <summary>
    /// Gets the normalized step used for pivot snapping.
    /// </summary>
    public Vector2 PivotSnapStep { get; }

    /// <summary>
    /// Gets the size of selection tool interaction handles.
    /// </summary>
    public float HandleSize { get; }
}