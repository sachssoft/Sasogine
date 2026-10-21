using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Controls.Drawing;
namespace Sachssoft.Engine.Controls;
/// <summary>Hosts ordered direct scene rendering inside a UI layout slot, with a borrowed image fallback.</summary>
/// <remarks>The application owns the renderer, textures and quality policy. Drawing changes no control state.</remarks>
public class SceneViewport : Control
{
    private Action<SceneViewportDrawContext>? _render;
    private Texture2D? _source;
    private bool _useDirectRendering;
    private Color? _compositeBackground;
    private static readonly PropertyChangedEventArgs RenderArgs = new(nameof(Render));
    private static readonly PropertyChangedEventArgs SourceArgs = new(nameof(Source));
    private static readonly PropertyChangedEventArgs CompositeArgs = new(nameof(CompositeBackground));
    private static readonly PropertyChangedEventArgs DirectArgs = new(nameof(UseDirectRendering));
    /// <inheritdoc/>
    protected override string DefaultStyleName => "Image";
    /// <summary>Gets or sets the direct rendering callback. It runs between the preceding and following UI draws.</summary>
    public Action<SceneViewportDrawContext>? Render { get => _render; set => SetNotify(ref _render, value, RenderArgs); }
    /// <summary>Gets or sets the borrowed fallback texture, stretched to the content area.</summary>
    public Texture2D? Source { get => _source; set => SetNotify(ref _source, value, SourceArgs); }
    /// <summary>Gets or sets whether drawing invokes Render instead of sampling Source.</summary>
    public bool UseDirectRendering { get => _useDirectRendering; set => SetNotify(ref _useDirectRendering, value, DirectArgs); }
    /// <summary>Gets or sets the uniform opaque UI backdrop used to reproduce premultiplied image compositing.</summary>
    /// <remarks>Null keeps the scene's direct color output. A supplied color must match the underlying UI.</remarks>
    public Color? CompositeBackground { get => _compositeBackground; set => SetNotify(ref _compositeBackground, value, CompositeArgs); }
    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
        => Source is { } source ? new Size2(source.Width, source.Height) : Size2.Zero;
    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        Bounds2 content = new(Padding.Left, Padding.Top, ContentBounds.Width, ContentBounds.Height);
        using (context.PushClipScope(content))
        {
            if (UseDirectRendering && Render is { } render) context.DrawScene(content, render, CompositeBackground);
            else if (Source is { } source) context.DrawTexture(source, source.Bounds, content, Color.White);
        }
    }
}
