using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using sachssoft.Graphics.Primitives;
using sachssoft.Graphics.Renderer;
using sachssoft.Sasogine.Diagnostics;
using sachssoft.Sasogine.Editor.Tiling.Tools;
using sachssoft.Sasogine.Graphics;
using sachssoft.Sasogine.Interactions;
using sachssoft.Sasogine.Tiling;
using sachssoft.Sasogine.Tiling.Stacked;
using sachssoft.Sasogine.Views.Editor;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace sachssoft.Sasogine.Editor.Tiling;

public abstract class TileEditorRuntimeBase : RuntimeBase
{
    private Vector2 _last_cursor_position;
    private Vector2 _current_cursor_position;
    private readonly Dictionary<Type, (EditorToolCursor Instance, EditorToolMetadata Metadata)> _registered_tools = new();
    private readonly Interaction<TileEditorCursorInteractions> _interaction = new();
    private readonly AxisInput<TileEditorCursorAxisInputs> _axis = new();
    private double _benchmark_display_cooldown;

    private IEditorTileMap? _map;
    private GridPrimitive? _grid_primitive;
    private TilePrimitive? _tile_primitive;
    private Texture2D? _cursor_tile;
    private Vector2 _tile_size = new(10f);
    private EditorToolCursor _tool_cursor;

    public event EventHandler? MapChanged;

    protected TileEditorRuntimeBase(OrthographicTileCamera camera, IEffect? effect) : base(camera, effect)
    {
        ToolCursor = new EditorToolCursor();
    }

    protected TileEditorRuntimeBase() : base(CreateCamera(), CreateEffect()) { }

    protected IEnumerable<EditorToolCursor> Tools => _registered_tools.Values.Select(x => x.Instance);

    protected IEnumerable<EditorToolMetadata> ToolMetadata => _registered_tools.Values.Select(x => x.Metadata);

    public IEditorTileMap? Map
    {
        get => _map;
        set
        {
            if (_map != value)
            {
                _map?.Dispose();
                _map = value;
                _map?.Initialize();
                UpdateGridFromMap();
                MapChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public GridArrangement GridDrawOrder { get; set; } = GridArrangement.InFrontOfTiles;

    public Vector2 TileSize
    {
        get => _tile_size;
        set
        {
            if (_tile_size != value)
            {
                _tile_size = value;
                UpdateGridFromMap();
            }
        }
    }

    public bool GridVisibilty { get; set; } = true;

    public int EditableLayerIndex { get; set; }

    public EditorToolCursor ToolCursor
    {
        get => _tool_cursor;
        set => _tool_cursor = value ?? new EditorToolCursor();
    }

    public new OrthographicTileCamera Camera => (OrthographicTileCamera)base.Camera;

    public float BenchmarkFrameRate { get; set; } = 0.1f;

    public Benchmark? Benchmark { get; set; } = new();

    public IDebugDisplay? DebugDisplay { get; set; }

    public Interaction<TileEditorCursorInteractions> Interaction => _interaction;

    public AxisInput<TileEditorCursorAxisInputs> Axis => _axis;

    public float CameraMoveSpeed { get; set; }

    public EditorDiagnosticsDisplayFlags DiagnosticsFlags { get; set; } =
        EditorDiagnosticsDisplayFlags.FramePerSecond |
        EditorDiagnosticsDisplayFlags.MapRender | EditorDiagnosticsDisplayFlags.MapUpdate |
        EditorDiagnosticsDisplayFlags.ToolCursor | EditorDiagnosticsDisplayFlags.Surface |
        EditorDiagnosticsDisplayFlags.TileIdentifier |
        EditorDiagnosticsDisplayFlags.CameraPosition | EditorDiagnosticsDisplayFlags.CameraZoom;

    public IEditorCursorAction? ToolCursorAction { get; set; }

    public int SelectedLayerIndex { get; set; }

    public override void Load()
    {
        base.Load();
        _tile_primitive = new TilePrimitive();
        _cursor_tile = IMyGameApp.Current.GraphicsDevice.CreateEmptyTexture(Color.White);
        UpdateGridFromMap();
    }

    public override void Unload()
    {
        _tile_primitive?.Dispose();
        _grid_primitive?.Dispose();
        Map?.Dispose();
        base.Unload();
    }

    public override void Update(GameContext context)
    {
        base.Update(context);

        UpdateMap(context);
        HandleMoveCamera(context);
        HandleEditorZoomCamera(context);
        HandleCursor(context);
        UpdateDiagnosticsDisplay(context);

        _interaction.Update();
    }

    protected override void OnScreenDraw(GameContext context)
    {
        base.OnScreenDraw(context);

        if (_tile_primitive == null) return;

        if (Map != null && Effect != null && GridVisibilty)
        {
            using var renderer = new TileRenderer(Camera, Effect, TileSize);

            var options = new TileDrawingOptions { TileSize = TileSize };
            var renderer_context = new TileRendererContext(context, renderer, _tile_primitive);

            OnTileDrawBefore(renderer_context);

            if (GridDrawOrder == GridArrangement.BehindTiles)
            {
                _grid_primitive?.Draw(Effect, Camera, Matrix.Identity);
                DrawMap(renderer, context, options);
            }
            else
            {
                DrawMap(renderer, context, options);
                _grid_primitive?.Draw(Effect, Camera, Matrix.Identity);
            }

            OnTileDrawAfter(renderer_context);

            if (_cursor_tile != null && !IsAnySurfaceHovered)
                ToolCursor.Draw(context, _tile_primitive, renderer, _cursor_tile);
        }
    }

    protected virtual void OnTileDrawBefore(TileRendererContext context) { }

    protected virtual void OnTileDrawAfter(TileRendererContext context) { }

    private static OrthographicTileCamera CreateCamera() => new();

    private static ImplementedBasicEffect CreateEffect() => new();

    private void HandleMoveCamera(GameContext context)
    {
        _last_cursor_position = _current_cursor_position;
        _current_cursor_position = _axis.Get(TileEditorCursorAxisInputs.Move);

        if (IsAnySurfaceHovered) return;

        if (_interaction.IsPressed(TileEditorCursorInteractions.CameraMove))
        {
            var delta = _current_cursor_position - _last_cursor_position;
            Camera.Position += delta * new Vector2(-1f, 1f) * Camera.Zoom / Camera.ZoomFactor;
        }

        if (_interaction.IsPressed(TileEditorCursorInteractions.CameraMoveLeft)) Camera.Move(new Vector2(CameraMoveSpeed, 0f));
        if (_interaction.IsPressed(TileEditorCursorInteractions.CameraMoveRight)) Camera.Move(new Vector2(-CameraMoveSpeed, 0f));
        if (_interaction.IsPressed(TileEditorCursorInteractions.CameraMoveUp)) Camera.Move(new Vector2(0f, -CameraMoveSpeed));
        if (_interaction.IsPressed(TileEditorCursorInteractions.CameraMoveDown)) Camera.Move(new Vector2(0f, CameraMoveSpeed));
    }

    private void HandleEditorZoomCamera(GameContext context)
    {
        if (IsAnySurfaceHovered) return;

        if (_interaction.IsPressed(TileEditorCursorInteractions.CameraZoomIn)) Camera.ZoomIn();
        else if (_interaction.IsPressed(TileEditorCursorInteractions.CameraZoomOut)) Camera.ZoomOut();
        else if (_interaction.IsPressed(TileEditorCursorInteractions.CameraZoomReset)) Camera.ResetZoom();
    }

    private void HandleCursor(GameContext context)
    {
        if (_interaction.WasJustPressed(TileEditorCursorInteractions.ToolCursor)) ToolCursor.BeginInteraction();
        else if (_interaction.WasJustReleased(TileEditorCursorInteractions.ToolCursor)) ToolCursor.EndInteraction();

        if (!IsAnySurfaceHovered) ToolCursor.Update(this, context);
    }

    private void UpdateDiagnosticsDisplay(GameContext context)
    {
        if (DiagnosticsFlags == EditorDiagnosticsDisplayFlags.None) return;

        var delta_seconds = context.GameTime.ElapsedGameTime.TotalSeconds;
        _benchmark_display_cooldown -= delta_seconds;
        if (_benchmark_display_cooldown > 0) return;

        _benchmark_display_cooldown = BenchmarkFrameRate;
        var sb = new StringBuilder();

        if (DiagnosticsFlags.HasFlag(EditorDiagnosticsDisplayFlags.FramePerSecond))
            sb.AppendLine($"FPS: {context.FrameCounter.AverageFramesPerSecond:0.0}");

        if (Benchmark != null)
        {
            if (DiagnosticsFlags.HasFlag(EditorDiagnosticsDisplayFlags.MapUpdate))
                sb.AppendLine($"Map Update: {Benchmark.GetScopedMeasurement(TilingDefinitions.BENCHMARK_MAP_UPDATE_MEASURE).TotalMicroseconds:0.0} µs");

            if (DiagnosticsFlags.HasFlag(EditorDiagnosticsDisplayFlags.MapRender))
                sb.AppendLine($"Map Render: {Benchmark.GetScopedMeasurement(TilingDefinitions.BENCHMARK_MAP_RENDER_MEASURE).TotalMilliseconds:0.0} ms");
        }

        if (DiagnosticsFlags.HasFlag(EditorDiagnosticsDisplayFlags.ToolCursor))
        {
            sb.AppendLine($"Tool Cursor: {ToolCursor.GetType().Name}");
            sb.AppendLine($"Tool Cursor Size: {ToolCursor.Size}");
            sb.AppendLine($"Tool Cursor Scope: {ToolCursor.Scope}");
        }

        if (DiagnosticsFlags.HasFlag(EditorDiagnosticsDisplayFlags.Surface))
            sb.AppendLine($"Surface Hovered: {(IsAnySurfaceHovered ? "Yes" : "No")}");

        if (DiagnosticsFlags.HasFlag(EditorDiagnosticsDisplayFlags.CameraPosition))
            sb.AppendLine($"Camera Position: {Camera.Position}");

        if (DiagnosticsFlags.HasFlag(EditorDiagnosticsDisplayFlags.CameraZoom))
            sb.AppendLine($"Camera Zoom: {Camera.Zoom}");

        DebugDisplay?.SendDebugText(this, sb.ToString());
        DebugDisplay?.Update(context);
    }

    private void UpdateMap(GameContext context)
    {
        if (Benchmark == null)
        {
            Map?.Update(context);
            return;
        }

        using var measure = new BenchmarkScope(Benchmark, TilingDefinitions.BENCHMARK_MAP_UPDATE_MEASURE);
        Map?.Update(context);
    }

    private void DrawMap(TileRenderer renderer, GameContext context, TileDrawingOptions options)
    {
        var view_bounds = TilingHelper.GetViewBounds(IMyGameApp.Current.GraphicsDevice, Camera, TileSize);

        if (Benchmark == null)
        {
            Map?.Draw(renderer, _tile_primitive!, context, options, view_bounds);
            return;
        }

        using var measure = new BenchmarkScope(Benchmark, TilingDefinitions.BENCHMARK_MAP_RENDER_MEASURE);
        Map?.Draw(renderer, _tile_primitive!, context, options, view_bounds);
    }

    private void UpdateGridFromMap()
    {
        _grid_primitive ??= new GridPrimitive();
        _grid_primitive.Size = TileSize;
        _grid_primitive.Offset = TileSize / -2f;

        if (_map != null)
        {
            _grid_primitive.Columns = _map.Columns;
            _grid_primitive.Rows = _map.Rows;
            _grid_primitive.Build();
        }
    }

    public void SelectTool<TTool>(Func<TTool>? factory = null) where TTool : EditorToolCursor, new() { }

    public void UnselectTool() => ToolCursor = null;
}