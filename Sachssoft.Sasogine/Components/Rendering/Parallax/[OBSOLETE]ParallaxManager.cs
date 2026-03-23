using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Rendering.Camera;
using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Graphics.Primitives;
using Sachssoft.Sasogine.Graphics.Rendering;
using Sachssoft.Sasogine.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Components.Rendering.Parallax
{
    [Obsolete("This class is a first prototype and may be removed or redesigned in the future. Use with caution.")]
    public class ParallaxManager
    {
        private readonly QuadPrimitive _displayPrimitive = new QuadPrimitive(Vector2.Zero, Vector2.One, Color.White, FlipMode.Vertical);
        private IEnumerable<ParallaxEntry>? _entries;
        private List<ParallaxEntry> _currentEntries = new List<ParallaxEntry>();
        private string? _currentLayer;
        private Vector2 _scrollingOffet;

        public Vector2 AreaSize { get; set; } = Vector2.One;

        public float PixelsPerUnit { get; set; } = 8f;

        public RenderOptions RenderOptions { get; set; } = RenderOptions.ScenePremultiplied;

        public ICamera? CustomCamera { get; set; }

        public IEffectAdapter? CustomEffect { get; set; }

        public Color TintColor { get; set; } = Color.White;

        public IEnumerable<ParallaxEntry>? Entries
        {
            get => _entries;
            set
            {
                if (_entries != value)
                {
                    _entries = value;
                    RebuildCurrentEntries();
                }
            }
        }

        public string? CurrentLayer
        {
            get => _currentLayer;
            set
            {
                if (_currentLayer != value)
                {
                    _currentLayer = value;
                    RebuildCurrentEntries();
                }
            }
        }

        private void RebuildCurrentEntries()
        {
            _currentEntries.Clear();

            if (_entries == null || !_entries.Any())
                return;

            if (_currentLayer == null)
            {
                _currentEntries.AddRange(_entries);
                return;
            }

            foreach (var entry in _entries)
            {
                if (entry.Layer == _currentLayer)
                    _currentEntries.Add(entry);
            }
        }

        public void Draw(RuntimeViewportContext context)
        {
            if (_displayPrimitive == null ||
                _currentEntries == null ||
                _currentEntries.Count == 0)
                return;

            var effectAdapter = context.EffectAdapter;

            // courseSize ist bereits in Weltkoordinaten (wie du gesagt hast)
            var courseWorldSize = AreaSize;

            // Kamera-Weltposition (für Parallax-Scroll)
            var camera = CustomCamera ?? context.Camera;
            var camera2D = camera as Camera2D;
            var cameraPos = camera2D !=null ? camera2D.Position : Vector2.Zero;

            for (int k = 0; k < _currentEntries.Count; k++)
            {
                var entry = _currentEntries[k];
                var texture = entry.Texture;
                if (texture == null)
                    continue;

                Matrix depth = MatrixHelper.CreateDepth(EntryDepthOverride(k, entry.Depth));

                // Texturgröße in Weltkoordinaten (inkl. optionaler TextureScale)
                var textureWorldSize = new Vector2(texture.Width, texture.Height) * entry.TextureScale / PixelsPerUnit;

                // Abstand zwischen Kacheln (Spacing in Weltkoordinaten)
                var spacingWorld = entry.Spacing / PixelsPerUnit;
                var tileStep = textureWorldSize + spacingWorld;
                if (tileStep.X <= 0f) tileStep.X = 1f;
                if (tileStep.Y <= 0f) tileStep.Y = 1f;

                // Basis-Position (Alignment innerhalb des Kurses) — ohne ScrollOffset
                var basePosition = GetAlignmentPosition(textureWorldSize, entry.HorizontalAlignment, entry.VerticalAlignment);

                // ScrollOffset aufgrund Kamera (Parallax)
                Vector2 scrollOffset = Vector2.Zero;

                switch (entry.ScrollingBehavior)
                {
                    case ScrollingBehavior.None:
                        scrollOffset = Vector2.Zero;
                        break;

                    case ScrollingBehavior.Static:
                        // Texture steht wie im Bildschirm still
                        break;

                    case ScrollingBehavior.Scrolling:
                        _scrollingOffet += entry.ScrollSpeed * context.ElapsedTimeInSeconds;
                        scrollOffset = _scrollingOffet;
                        break;

                    case ScrollingBehavior.Follow:
                        scrollOffset = -cameraPos * entry.ScrollSpeed;
                        break;
                }

                // Gesamtoffset (Entry.Offset ist in Weltkoordinaten angenommen)
                var manualOffset = entry.Offset;

                // Wenn keine Wiederholung gewünscht ist, zeichne genau eine Instanz an der Basisposition + Offsets
                if (entry.RepeatMode == RepeatMode.None)
                {
                    var pos = basePosition + manualOffset + scrollOffset;

                    Matrix transform =
                        Matrix.CreateScale(textureWorldSize.X, textureWorldSize.Y, 1f) *
                        Matrix.CreateTranslation(pos.X, pos.Y, 0f) *
                        depth;

                    DrawOverride(context, texture, effectAdapter, transform);
                    continue;
                }

                // --- Wiederholung: berechne Indexbereich, der den gesamten Kurs abdeckt ---
                // Ziel: finde iMin..iMax und jMin..jMax so dass für jeden i,j die Kachel
                // at = basePosition + (i*tileStep.X, j*tileStep.Y) + manualOffset + scrollOffset
                // den Bereich [0, courseWorldSize] (in Weltkoordinaten) schneiden kann.

                // Wir lösen Ungleichungen für i:
                // at.X + tileWorldWidth > 0  AND at.X < courseWorldSize.X
                // => baseX + i*tileStepX + manualX + scrollX < courseW
                //    baseX + i*tileStepX + manualX + scrollX + tileW > 0
                // => i < (courseW - baseX - manualX - scrollX) / tileStepX
                // => i > (- tileW - baseX - manualX - scrollX) / tileStepX

                float baseX = basePosition.X;
                float baseY = basePosition.Y;
                float manualX = manualOffset.X;
                float manualY = manualOffset.Y;
                float scrollX = scrollOffset.X;
                float scrollY = scrollOffset.Y;
                float tileW = textureWorldSize.X;
                float tileH = textureWorldSize.Y;
                float stepX = tileStep.X;
                float stepY = tileStep.Y;

                // Rechne int-Grenzen (inklusive Randkacheln)
                int iMin, iMax, jMin, jMax;

                if (entry.RepeatMode == RepeatMode.RepeatX || entry.RepeatMode == RepeatMode.RepeatXY)
                {
                    var iMinF = float.Floor((-tileW - baseX - manualX - scrollX) / stepX);
                    var iMaxF = float.Ceiling((courseWorldSize.X - baseX - manualX - scrollX) / stepX);
                    iMin = (int)iMinF - 1; // -1 Sicherheitsmargin
                    iMax = (int)iMaxF + 1; // +1 Sicherheitsmargin
                }
                else
                {
                    iMin = 0; iMax = 0;
                }

                if (entry.RepeatMode == RepeatMode.RepeatY || entry.RepeatMode == RepeatMode.RepeatXY)
                {
                    var jMinF = float.Floor((-tileH - baseY - manualY - scrollY) / stepY);
                    var jMaxF = float.Ceiling((courseWorldSize.Y - baseY - manualY - scrollY) / stepY);
                    jMin = (int)jMinF - 1;
                    jMax = (int)jMaxF + 1;
                }
                else
                {
                    jMin = 0; jMax = 0;
                }

                for (int i = iMin; i <= iMax; i++)
                {
                    for (int j = jMin; j <= jMax; j++)
                    {
                        // wenn RepeatX/Y nicht gesetzt, dann i/j müssen =0 sein (sicherheitscheck)
                        if ((entry.RepeatMode == RepeatMode.RepeatX || entry.RepeatMode == RepeatMode.RepeatXY) == false && i != 0)
                            continue;
                        if ((entry.RepeatMode == RepeatMode.RepeatY || entry.RepeatMode == RepeatMode.RepeatXY) == false && j != 0)
                            continue;

                        var pos = new Vector2(
                            baseX + i * stepX + manualX + scrollX,
                            baseY + j * stepY + manualY + scrollY
                        );

                        // Frustum-/AABB-Culling: nur wenn Kachel tatsächlich den Kurs schneidet
                        if (pos.X + tileW < 0f || pos.X > courseWorldSize.X ||
                            pos.Y + tileH < 0f || pos.Y > courseWorldSize.Y)
                            continue;

                        Matrix transform =
                            Matrix.CreateScale(tileW, tileH, 1f) *
                            Matrix.CreateTranslation(pos.X, pos.Y, 0f) *
                            depth;

                        DrawOverride(context, texture, effectAdapter, transform);
                    }
                }
            }
        }

        protected virtual void DrawOverride(RuntimeViewportContext context, Texture2D? texture, IEffectAdapter effect, Matrix transform)
        {
            effect.Color = TintColor;
            effect.Texture = texture;

            _displayPrimitive.DrawScoped(context, transform: transform, options: RenderOptions);

            effect.Texture = null;
            effect.Color = default;
        }

        protected virtual float EntryDepthOverride(int entryIndex, float baseDepth)
        {
            return entryIndex + 0.01f * baseDepth;
        }

        private Vector2 GetAlignmentPosition(Vector2 textureSize, Alignment halign, Alignment valign)
        {
            float x = 0f;
            float y = 0f;

            // Horizontal
            if (halign == Alignment.Center)
                x = (AreaSize.X - textureSize.X) * 0.5f;
            else if (halign == Alignment.Far)
                x = AreaSize.X - textureSize.X;

            // Vertical
            if (valign == Alignment.Center)
                y = (AreaSize.Y - textureSize.Y) * 0.5f;
            else if (valign == Alignment.Far)
                y = AreaSize.Y - textureSize.Y;

            return new Vector2(x, y);
        }

    }
}
