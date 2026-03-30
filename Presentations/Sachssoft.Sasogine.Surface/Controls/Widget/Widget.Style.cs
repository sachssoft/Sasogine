using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Interactions;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public partial class Widget
    {
        private string? _styleId;
        private Style? _style;
        private Widget? _parent;

        #region Direct Properties

        public string? StyleId
        {
            get => _styleId;
            protected set => _styleId = value;
        }

        public Style? Style
        {
            get => _style;
        }

        #endregion

        // Wenn StyleName / StyleId geändert, sollte man manuell ApplyStyle ausführen
        public void ApplyStyle()
        {
            ApplyFromStyle(Style);
        }

        public virtual void ApplyFromStyle(Style? style)
        {
            style?.Apply(this, (target, sheet, property, value) =>
            {
                switch (property)
                {
                    // Größe
                    case nameof(Width):
                        target.Width = target.Width.Override(value.ConvertTo<int>(0));
                        break;
                    case nameof(Height):
                        target.Height = target.Height.Override(value.ConvertTo<int>(0));
                        break;
                    case nameof(MinWidth):
                        target.MinWidth = target.MinWidth.Override(value.ConvertTo<int>(0));
                        break;
                    case nameof(MinHeight):
                        target.MinHeight = target.MinHeight.Override(value.ConvertTo<int>(0));
                        break;
                    case nameof(MaxWidth):
                        target.MaxWidth = target.MaxWidth.Override(value.ConvertTo<int>(0));
                        break;
                    case nameof(MaxHeight):
                        target.MaxHeight = target.MaxHeight.Override(value.ConvertTo<int>(0));
                        break;

                    // Hintergrund / Border
                    case nameof(Background):
                        target.Background = target.Background.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                        break;
                    case nameof(HoveredBackground):
                        target.HoveredBackground = target.HoveredBackground.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                        break;
                    case nameof(DisabledBackground):
                        target.DisabledBackground = target.DisabledBackground.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                        break;
                    case nameof(FocusedBackground):
                        target.FocusedBackground = target.FocusedBackground.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                        break;

                    case nameof(Border):
                        target.Border = target.Border.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                        break;
                    case nameof(HoveredBorder):
                        target.HoveredBorder = target.HoveredBorder.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                        break;
                    case nameof(DisabledBorder):
                        target.DisabledBorder = target.DisabledBorder.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                        break;
                    case nameof(FocusedBorder):
                        target.FocusedBorder = target.FocusedBorder.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                        break;

                    // Font
                    case nameof(Font):
                        target.Font = target.Font.Override(sheet.GetFont(value.RawValue));
                        break;

                    // Text
                    case nameof(TextColor):
                        target.TextColor = target.TextColor.Override(value.ConvertTo<Color>());
                        break;
                    case nameof(DisabledTextColor):
                        target.DisabledTextColor = target.DisabledTextColor.Override(value.ConvertTo<Color>());
                        break;
                    case nameof(OverTextColor):
                        target.OverTextColor = target.OverTextColor.Override(value.ConvertTo<Color>());
                        break;
                    case nameof(PressedTextColor):
                        target.PressedTextColor = target.PressedTextColor.Override(value.ConvertTo<Color>());
                        break;
                    case nameof(TextHorizontalAlignment):
                        target.TextHorizontalAlignment = target.TextHorizontalAlignment.Override(value.ConvertToEnum<HorizontalAlignment>());
                        break;
                    case nameof(TextVerticalAlignment):
                        target.TextVerticalAlignment = target.TextVerticalAlignment.Override(value.ConvertToEnum<VerticalAlignment>());
                        break;

                    // Fokus / Keyboard / Z-Index
                    case nameof(ZIndex):
                        target.ZIndex = target.ZIndex.Override(value.ConvertTo<int>());
                        break;
                    case nameof(AcceptsKeyboardFocus):
                        target.AcceptsKeyboardFocus = target.AcceptsKeyboardFocus.Override(value.ConvertTo<bool>());
                        break;

                    // Margin / Padding / BorderThickness
                    case nameof(Margin):
                        target.Margin = target.Margin.Override(value.ConvertTo<Thickness>());
                        break;
                    case nameof(Padding):
                        target.Padding = target.Padding.Override(value.ConvertTo<Thickness>());
                        break;
                    case nameof(BorderThickness):
                        target.BorderThickness = target.BorderThickness.Override(value.ConvertTo<Thickness>());
                        break;

                    // Alignment
                    case nameof(HorizontalAlignment):
                        target.HorizontalAlignment = target.HorizontalAlignment.Override(value.ConvertToEnum<HorizontalAlignment>());
                        break;
                    case nameof(VerticalAlignment):
                        target.VerticalAlignment = target.VerticalAlignment.Override(value.ConvertToEnum<VerticalAlignment>());
                        break;

                    // Visibility / Enabled
                    case nameof(IsVisible):
                        target.IsVisible = target.IsVisible.Override(value.ConvertTo<bool>());
                        break;
                    case nameof(IsEnabled):
                        target.IsEnabled = target.IsEnabled.Override(value.ConvertTo<bool>());
                        break;

                    // Mouse / Drag
                    case nameof(MouseCursor):
                        target.MouseCursor = target.MouseCursor.Override(value.ConvertToEnum<MouseCursorType>());
                        break;
                    case nameof(DragDirection):
                        target.DragDirection = target.DragDirection.Override(value.ConvertToEnum<DragDirection>());
                        break;
                    case nameof(ClipToBounds):
                        target.ClipToBounds = target.ClipToBounds.Override(value.ConvertTo<bool>());
                        break;

                    // Transform / Visuals
                    case nameof(Scale):
                        target.Scale = target.Scale.Override(value.ConvertTo<Vector2>());
                        break;
                    case nameof(TransformOrigin):
                        target.TransformOrigin = target.TransformOrigin.Override(value.ConvertTo<Vector2>());
                        break;
                    case nameof(Rotation):
                        target.Rotation = target.Rotation.Override(value.ConvertTo<float>());
                        break;
                    case nameof(Opacity):
                        target.Opacity = target.Opacity.Override(value.ConvertTo<float>());
                        break;
                }
            });

            if (style != null && style.HasPropertiesFor<Font>())
            {
                Font = style.BuildFor<Font>();
            }
        }

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not Widget source)
                return;

            _styleId = source.StyleId;

            // Position & Größe
            Left = source.Left;
            Top = source.Top;
            MinWidth = source.MinWidth;
            MaxWidth = source.MaxWidth;
            Width = source.Width;
            MinHeight = source.MinHeight;
            MaxHeight = source.MaxHeight;
            Height = source.Height;

            // Layout
            Margin = source.Margin;
            BorderThickness = source.BorderThickness;
            Padding = source.Padding;
            HorizontalAlignment = source.HorizontalAlignment;
            VerticalAlignment = source.VerticalAlignment;
            ZIndex = source.ZIndex;

            // Sichtbarkeit & Aktivierung
            IsVisible = source.IsVisible;
            IsEnabled = source.IsEnabled;
            ClipToBounds = source.ClipToBounds;
            AcceptsKeyboardFocus = source.AcceptsKeyboardFocus;

            // Drag & Mouse
            DragDirection = source.DragDirection;
            MouseCursor = source.MouseCursor;

            // Transform / Visuals
            Scale = source.Scale;
            TransformOrigin = source.TransformOrigin;
            Rotation = source.Rotation;
            Opacity = source.Opacity;

            // Hintergrund & Rahmen
            Background = source.Background;
            HoveredBackground = source.HoveredBackground;
            DisabledBackground = source.DisabledBackground;
            FocusedBackground = source.FocusedBackground;
            Border = source.Border;
            HoveredBorder = source.HoveredBorder;
            DisabledBorder = source.DisabledBorder;
            FocusedBorder = source.FocusedBorder;

            // Text
            Font = source.Font;
            TextHorizontalAlignment = source.TextHorizontalAlignment;
            TextVerticalAlignment = source.TextVerticalAlignment;
            TextColor = source.TextColor;
            DisabledTextColor = source.DisabledTextColor;
            OverTextColor = source.OverTextColor;
            PressedTextColor = source.PressedTextColor;
        }

        protected override ElementBase CreateCloneInstance()
        {
            return new Widget();
        }

        private void EnsureLoadStyle()
        {
            var originalType = GetType();
            var type = originalType;

            // Suche mit StyleId
            while (type != null)
            {
                _style = Stylesheet.Current.FindStyle(type, _styleId);
                if (_style != null)
                    break;

                type = type.BaseType;
            }

            // Falls nichts gefunden: Suche ohne StyleId
            if (_style == null)
            {
                type = originalType;

                while (type != null)
                {
                    _style = Stylesheet.Current.FindStyle(type);
                    if (_style != null)
                        break;

                    type = type.BaseType;
                }
            }

            if (_style != null)
            {
                ApplyFromStyle(_style);
            }
        }
    }
}
