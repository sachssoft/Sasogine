
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace Sachssoft.Engine.Diagnostics;

/// <summary>
/// Displays a real-time frame rate history graph for performance diagnostics.
/// </summary>
public sealed class FrameCounterGraph : IDisposable
{
    private const int SampleCapacity = 240;
    private const double SampleInterval = 0.25;

    private readonly Texture2D _pixel;
    private readonly float[] _samples = new float[SampleCapacity];

    private long _lastTimestamp;
    private double _sampleTime;
    private int _frameCount;
    private int _nextIndex;
    private int _count;

    /// <summary>
    /// Gets the most recently measured frame rate in frames per second.
    /// </summary>
    public float FrameRate { get; private set; }

    /// <summary>
    /// Gets the average frame duration in milliseconds
    /// based on the most recent frame rate measurement.
    /// </summary>
    public float AverageFrameTime => FrameRate > 0f
        ? 1000f / FrameRate
        : 0f;

    /// <summary>
    /// Gets or sets whether the graph is visible.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the frame counter graph.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device used for rendering.</param>
    public FrameCounterGraph(GraphicsDevice graphicsDevice)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    /// <summary>
    /// Records a rendered frame using real elapsed time.
    /// Call this method once per rendered frame.
    /// </summary>
    public void RecordFrame()
    {
        long timestamp = Stopwatch.GetTimestamp();

        if (_lastTimestamp == 0)
        {
            _lastTimestamp = timestamp;
            return;
        }

        double elapsed = (double)(timestamp - _lastTimestamp) / Stopwatch.Frequency;
        _lastTimestamp = timestamp;

        if (elapsed <= 0.0 || elapsed > 2.0)
        {
            _sampleTime = 0.0;
            _frameCount = 0;
            return;
        }

        _sampleTime += elapsed;
        _frameCount++;

        if (_sampleTime < SampleInterval)
            return;

        FrameRate = (float)(_frameCount / _sampleTime);

        _samples[_nextIndex] = FrameRate;
        _nextIndex = (_nextIndex + 1) % SampleCapacity;
        _count = Math.Min(_count + 1, SampleCapacity);

        _sampleTime = 0.0;
        _frameCount = 0;
    }

    /// <summary>
    /// Draws the frame rate history graph in screen coordinates.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch used for rendering.</param>
    /// <param name="bounds">The screen rectangle occupied by the graph.</param>
    /// <param name="font">The optional font used for displaying frame statistics.</param>
    public void Draw(SpriteBatch spriteBatch, Rectangle bounds, SpriteFont? font = null)
    {
        if (!Visible || bounds.Width < 80 || bounds.Height < 60)
            return;

        ArgumentNullException.ThrowIfNull(spriteBatch);

        int headerHeight = font != null ? 32 : 12;

        var plot = new Rectangle(
            bounds.X + 14,
            bounds.Y + headerHeight,
            bounds.Width - 28,
            bounds.Height - headerHeight - 12);

        if (plot.Width <= 1 || plot.Height <= 1)
            return;

        float maxFrameRate = 60f;

        for (int i = 0; i < _count; i++)
            maxFrameRate = Math.Max(maxFrameRate, _samples[i]);

        maxFrameRate = MathF.Ceiling(maxFrameRate / 30f) * 30f;

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone);

        // Background.
        spriteBatch.Draw(
            _pixel,
            bounds,
            new Color(10, 15, 22, 210));

        // Horizontal frame rate reference lines.
        for (int frameRate = 30; frameRate <= maxFrameRate; frameRate += 30)
        {
            int y = GetY(plot, frameRate, maxFrameRate);

            spriteBatch.Draw(
                _pixel,
                new Rectangle(plot.Left, y, plot.Width, 1),
                new Color(255, 255, 255, 35));
        }

        // Graph axes.
        spriteBatch.Draw(
            _pixel,
            new Rectangle(plot.Left, plot.Top, 1, plot.Height),
            new Color(255, 255, 255, 90));

        spriteBatch.Draw(
            _pixel,
            new Rectangle(plot.Left, plot.Bottom - 1, plot.Width, 1),
            new Color(255, 255, 255, 90));

        // Frame rate history.
        if (_count >= 2)
        {
            int firstIndex = _count == SampleCapacity
                ? _nextIndex
                : 0;

            float step = (float)(plot.Width - 1) / (SampleCapacity - 1);

            for (int i = 1; i < _count; i++)
            {
                int previousIndex = (firstIndex + i - 1) % SampleCapacity;
                int currentIndex = (firstIndex + i) % SampleCapacity;

                float previousFrameRate = _samples[previousIndex];
                float currentFrameRate = _samples[currentIndex];

                float x1 = plot.Right - 1 - (_count - i) * step;
                float x2 = plot.Right - 1 - (_count - i - 1) * step;

                float y1 = GetY(plot, previousFrameRate, maxFrameRate);
                float y2 = GetY(plot, currentFrameRate, maxFrameRate);

                Color color = currentFrameRate switch
                {
                    < 30f => Color.IndianRed,
                    < 55f => Color.Gold,
                    _ => Color.LimeGreen
                };

                DrawLine(
                    spriteBatch,
                    new Vector2(x1, y1),
                    new Vector2(x2, y2),
                    color,
                    2f);
            }
        }

        // Frame statistics.
        if (font != null)
        {
            string label = $"FPS: {FrameRate:0.0}  |  {AverageFrameTime:0.0} ms";

            spriteBatch.DrawString(
                font,
                label,
                new Vector2(bounds.X + 12, bounds.Y + 7),
                Color.White);
        }

        spriteBatch.End();
    }

    private static int GetY(Rectangle plot, float frameRate, float maxFrameRate)
    {
        float normalized = MathHelper.Clamp(frameRate / maxFrameRate, 0f, 1f);

        return plot.Bottom - 1 -
            (int)(normalized * (plot.Height - 1));
    }

    private void DrawLine(
        SpriteBatch spriteBatch,
        Vector2 start,
        Vector2 end,
        Color color,
        float thickness)
    {
        Vector2 direction = end - start;
        float length = direction.Length();

        if (length <= 0f)
            return;

        float angle = MathF.Atan2(direction.Y, direction.X);

        spriteBatch.Draw(
            _pixel,
            start,
            null,
            color,
            angle,
            new Vector2(0f, 0.5f),
            new Vector2(length, thickness),
            SpriteEffects.None,
            0f);
    }

    /// <summary>
    /// Clears all recorded frame statistics and resets the graph.
    /// </summary>
    public void Reset()
    {
        Array.Clear(_samples);

        _lastTimestamp = 0;
        _sampleTime = 0.0;
        _frameCount = 0;
        _nextIndex = 0;
        _count = 0;

        FrameRate = 0f;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _pixel.Dispose();
    }
}
