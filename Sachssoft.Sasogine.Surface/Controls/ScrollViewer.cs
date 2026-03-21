using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Utility;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System;
using System.Diagnostics;

//
// Sasogine Surface (Myra-basiert)
// Update von Tobias Sachs am 10.07.2025
// Fix: Finger-Scrolling funktioniert jetzt korrekt.
//      Ursprünglich war Scrollen nur über den Thumb möglich –
//      direkte Fingerbewegung über den Content hatte keine oder invertierte Wirkung.
//      Die Prüfung auf Thumb wurde überarbeitet und unnötige Bedingungen entfernt.
//
namespace Sachssoft.Sasogine.Surface.Controls;

public class ScrollViewer : ContentControl
{
    private StyleProperty<bool> _isHorizontalScrollBarVisible = new StyleProperty<bool>(false, isUserSet: false);
    private StyleProperty<bool> _isVerticalScrollBarVisible = new StyleProperty<bool>(true, isUserSet: false);
    private StyleProperty<bool> _canScrollHorizontal = new StyleProperty<bool>(false, isUserSet: false);
    private StyleProperty<bool> _canScrollVertical = new StyleProperty<bool>(true, isUserSet: false);
    private StyleProperty<bool> _canDragScroll = new StyleProperty<bool>(true, isUserSet: false);
    private StyleProperty<ITextureRegion?> _horizontalScrollBackground = new StyleProperty<ITextureRegion?>(null, isUserSet: false);
    private StyleProperty<ITextureRegion?> _horizontalScrollThumb = new StyleProperty<ITextureRegion?>(null, isUserSet: false);
    private StyleProperty<ITextureRegion?> _verticalScrollBackground = new StyleProperty<ITextureRegion?>(null, isUserSet: false);
    private StyleProperty<ITextureRegion?> _verticalScrollThumb = new StyleProperty<ITextureRegion?>(null, isUserSet: false);
    private StyleProperty<int> _scrollSpeed = new StyleProperty<int>(10, isUserSet: false);

    private readonly SingleItemLayout<Widget> _layout;
    private ScrollingBehavior _scrollingBehavior = ScrollingBehavior.None;
    private bool _horizontalScrollingOn;
    private bool _verticalScrollingOn;
    private Rectangle _horizontalScrollbarFrame;
    private Rectangle _horizontalScrollbarThumb;
    private Rectangle _verticalScrollbarFrame;
    private Rectangle _verticalScrollbarThumb;
    private int _thumbMaximumX, _thumbMaximumY;
    private Point? _startBoundsPos;

    public ScrollViewer()
    {
        _layout = new SingleItemLayout<Widget>(this);
        LayoutContainer  = _layout;

        ClipToBounds = ClipToBounds.Override(true);
        HorizontalAlignment = HorizontalAlignment.Override(Visuals.HorizontalAlignment.Stretch);
        VerticalAlignment = VerticalAlignment.Override(Visuals.VerticalAlignment.Stretch);

        _horizontalScrollingOn = _verticalScrollingOn = false;

    }

    #region Style Properties

    // Früher ShowHorizontalScrollBar von Myra
    public StyleProperty<bool> IsHorizontalScrollBarVisible
    {
        get => _isHorizontalScrollBarVisible;
        set
        {
            if (SetAndNotify(ref _isHorizontalScrollBarVisible, value))
            {
                InvalidateMeasure();
            }
        }
    }

    public StyleProperty<bool> CanScrollHorizontal
    {
        get => _canScrollHorizontal;
        set
        {
            if (SetAndNotify(ref _canScrollHorizontal, value))
            {
                InvalidateMeasure();
            }
        }
    }

    // Früher ShowVerticalScrollBar von Myra
    public StyleProperty<bool> IsVerticalScrollBarVisible
    {
        get => _isVerticalScrollBarVisible;
        set
        {
            if (SetAndNotify(ref _isVerticalScrollBarVisible, value))
            {
                InvalidateMeasure();
            }
        }
    }

    public StyleProperty<bool> CanScrollVertical
    {
        get => _canScrollVertical;
        set
        {
            if (SetAndNotify(ref _canScrollVertical, value))
            {
                InvalidateMeasure();
            }
        }
    }

    public StyleProperty<ITextureRegion?> HorizontalScrollBackground
    {
        get => _horizontalScrollBackground;
        set => SetAndNotify(ref _horizontalScrollBackground, value);
    }

    public StyleProperty<ITextureRegion?> HorizontalScrollThumb
    {
        get => _horizontalScrollThumb;
        set => SetAndNotify(ref _horizontalScrollThumb, value);
    }

    public StyleProperty<ITextureRegion?> VerticalScrollBackground
    {
        get => _verticalScrollBackground;
        set => SetAndNotify(ref _verticalScrollBackground, value);
    }

    public StyleProperty<ITextureRegion?> VerticalScrollThumb
    {
        get => _verticalScrollThumb;
        set => SetAndNotify(ref _verticalScrollThumb, value);
    }

    public StyleProperty<int> ScrollSpeed
    {
        get => _scrollSpeed;
        set => SetAndNotify(ref _scrollSpeed, value);
    }

    public StyleProperty<bool> CanDragScroll
    {
        get => _canDragScroll;
        set => SetAndNotify(ref _canDragScroll, value);
    }

    #endregion

    #region Direct Properties

    public Point ScrollPosition
    {
        get
        {
            if (ContentPresenter == null)
                return Point.Zero;

            return new Point(-ContentPresenter.Left, -ContentPresenter.Top);
        }
        set
        {
            if (ContentPresenter == null)
                return;

            ContentPresenter.Left = -value.X;
            ContentPresenter.Top = -value.Y;
        }
    }

    public Point ScrollMaximum
    {
        get
        {
            if (ContentPresenter == null)
                return Point.Zero;

            var bounds = ActualBounds;
            var result = new Point(ContentPresenter.Bounds.Width - bounds.Width + VerticalThumbWidth,
                             ContentPresenter.Bounds.Height - bounds.Height + HorizontalThumbHeight);

            if (result.X < 0)
                result.X = 0;

            if (result.Y < 0)
                result.Y = 0;

            return result;
        }
    }

    public int HorizontalScrollbarHeight
    {
        get
        {
            var result = 0;

            if (HorizontalScrollBackground.Value != null)
            {
                result = HorizontalScrollBackground.Value.Size.Y;
            }

            if (HorizontalScrollThumb.Value != null && HorizontalScrollThumb.Value.Size.Y > result)
            {
                result = HorizontalScrollThumb.Value.Size.Y;
            }

            return result;
        }
    }

    public int VerticalScrollbarWidth
    {
        get
        {
            var result = 0;
            if (VerticalScrollBackground.Value != null)
            {
                result = VerticalScrollBackground.Value.Size.X;
            }

            if (VerticalScrollThumb.Value != null && VerticalScrollThumb.Value.Size.X > result)
            {
                result = VerticalScrollThumb.Value.Size.X;
            }

            return result;
        }
    }

    public override Desktop Desktop
    {
        get
        {
            return base.Desktop;
        }

        internal set
        {
            if (Desktop != null)
            {
                Desktop.TouchMoved -= DesktopTouchMoved;
                Desktop.TouchUp -= DesktopTouchUp;
            }

            base.Desktop = value;

            if (Desktop != null)
            {
                Desktop.TouchMoved += DesktopTouchMoved;
                Desktop.TouchUp += DesktopTouchUp;
            }
        }
    }

    internal int VerticalThumbWidth
    {
        get
        {
            return _verticalScrollingOn && IsVerticalScrollBarVisible ? _verticalScrollbarThumb.Width : 0;
        }
    }

    internal int HorizontalThumbHeight
    {
        get
        {
            return _horizontalScrollingOn && IsHorizontalScrollBarVisible ? _horizontalScrollbarThumb.Height : 0;
        }
    }

    internal Point ThumbPosition
    {
        get
        {
            var sp = ScrollPosition;
            var m = ScrollMaximum;

            var result = Point.Zero;

            if (m.X > 0)
                result.X = sp.X * _thumbMaximumX / m.X;

            if (m.Y > 0)
                result.Y = sp.Y * _thumbMaximumY / m.Y;

            return result;
        }
    }

    #endregion

    protected override void OnContentChanging(EventArgs e)
    {
        base.OnContentChanging(e);

        if (ContentPresenter != null)
        {
            ContentPresenter.PropertyChanged -= Presenter_PropertyChanged;
        }
    }

    protected override void OnContentChanged(EventArgs e)
    {
        base.OnContentChanged(e);

        if (ContentPresenter != null)
        {
            ContentPresenter.DragDirection = Controls.DragDirection.None;
            ContentPresenter.PropertyChanged += Presenter_PropertyChanged;
        }
        ResetScroll();
    }

    private void Presenter_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        var presenter = (Widget)sender!;

        switch (e.PropertyName)
        {
            // Blockieren
            case nameof(Widget.DragDirection):
                presenter.DragDirection = Controls.DragDirection.None;
                break;
        }
    }

    protected internal override bool AcceptsMouseWheel => _verticalScrollingOn;

    private void MoveThumb(int delta, bool directionHorizontal, bool directionVertical)
    {
        var scrollPosition = ScrollPosition;
        var scrollMaximum = ScrollMaximum;

        if (directionVertical && !directionHorizontal)
        {
            scrollPosition.Y = MathHelper.Clamp(scrollPosition.Y + delta, 0, scrollMaximum.Y);
        }
        else if (directionHorizontal && !directionVertical)
        {
            scrollPosition.X = MathHelper.Clamp(scrollPosition.X + delta, 0, scrollMaximum.X);
        }
        else
        {
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
            {
                scrollPosition.X = MathHelper.Clamp(scrollPosition.X + delta, 0, scrollMaximum.X);
            }
            else
            {
                scrollPosition.Y = MathHelper.Clamp(scrollPosition.Y + delta, 0, scrollMaximum.Y);
            }
        }

        ScrollPosition = scrollPosition;
    }

    internal protected override void OnTouchDown()
    {
        base.OnTouchDown();

        if (Desktop == null)
            return;

        var touchPosition = ToLocal(Desktop.TouchPosition.GetValueOrDefault());
        _scrollingBehavior = ScrollingBehavior.None;

        var r = _verticalScrollbarThumb;
        var thumbPosition = ThumbPosition;
        r.Y += thumbPosition.Y;

        if (_isVerticalScrollBarVisible.Value && _verticalScrollingOn && r.Contains(touchPosition))
        {
            _startBoundsPos = new Point(0, Desktop.TouchPosition.GetValueOrDefault().Y);
            _scrollingBehavior = ScrollingBehavior.VerticalThumb;
            return;
        }

        r = _horizontalScrollbarThumb;
        r.X += thumbPosition.X;
        if (_isHorizontalScrollBarVisible.Value && _horizontalScrollingOn && r.Contains(touchPosition))
        {
            _startBoundsPos = new Point(Desktop.TouchPosition.GetValueOrDefault().X, 0);
            _scrollingBehavior = ScrollingBehavior.HorizontalThumb;
            return;
        }

        // Wenn Finger-/Maus-Drag-Scroll erlaubt und kein Thumb getroffen wurde:
        if (_scrollingBehavior == ScrollingBehavior.None && CanDragScroll)
        {
            var pos = Desktop?.TouchPosition.GetValueOrDefault() ?? Point.Zero;
            _startBoundsPos = pos;
            _scrollingBehavior = ScrollingBehavior.DirectDrag;
        }
    }

    internal protected override void OnTouchUp()
    {
        base.OnTouchUp();

        _startBoundsPos = null;
        _scrollingBehavior = ScrollingBehavior.None;
    }

    internal protected override void OnMouseWheel(float delta)
    {
        if (_scrollingBehavior == ScrollingBehavior.DirectDrag)
            return;

        base.OnMouseWheel(delta);

        bool directionHorizontal = false, directionVertical = false;

        if (_canScrollVertical && _canScrollHorizontal)
        {
            directionHorizontal = true;
            directionVertical = true;
        }
        else if (_canScrollVertical && !_canScrollHorizontal)
        {
            directionVertical = true;
        }
        else
        {
            directionHorizontal = true;
        }

        var step = _scrollSpeed.Value * ScrollMaximum.Y / _thumbMaximumY;
        if (delta < 0)
        {
            MoveThumb(step, directionHorizontal, directionVertical);
        }
        else if (delta > 0)
        {
            MoveThumb(-step, directionHorizontal, directionVertical);
        }

        _scrollingBehavior = ScrollingBehavior.None;
    }

    public override void InternalRender(RenderContext context, GameTime t)
    {
        if (ContentPresenter == null || !ContentPresenter.IsVisible)
        {
            return;
        }

        // Render child
        base.InternalRender(context, t);

        var thumbPosition = ThumbPosition;
        if (_horizontalScrollingOn && IsHorizontalScrollBarVisible)
        {
            if (HorizontalScrollBackground.Value != null)
            {
                HorizontalScrollBackground.Value.Draw(context, _horizontalScrollbarFrame, Color.White);
            }

            var r = _horizontalScrollbarThumb;
            r.X += thumbPosition.X;
            HorizontalScrollThumb.Value?.Draw(context, r, Color.White);
        }

        if (_verticalScrollingOn && IsVerticalScrollBarVisible)
        {
            if (VerticalScrollBackground.Value != null)
            {
                VerticalScrollBackground.Value.Draw(context, _verticalScrollbarFrame, Color.White);
            }

            var r = _verticalScrollbarThumb;
            r.Y += thumbPosition.Y;
            VerticalScrollThumb.Value?.Draw(context, r, Color.White);
        }
    }

    protected override Point InternalMeasure(Point availableSize)
    {
        if (ContentPresenter == null)
            return Point.Zero;

        var measureSize = ContentPresenter.Measure(availableSize);

        var horizontalScrollbarVisible = IsHorizontalScrollBarVisible && measureSize.X > availableSize.X;
        var verticalScrollbarVisible = IsVerticalScrollBarVisible && measureSize.Y > availableSize.Y;

        if (horizontalScrollbarVisible || verticalScrollbarVisible)
        {
            if (horizontalScrollbarVisible)
            {
                measureSize.Y += HorizontalScrollbarHeight;
            }

            if (verticalScrollbarVisible)
            {
                measureSize.X += VerticalScrollbarWidth;
            }
        }

        return measureSize;
    }

    protected override void InternalArrange()
    {
        if (ContentPresenter == null)
            return;

        var bounds = ActualBounds;
        var availableSize = bounds.Size();
        var oldMeasureSize = ContentPresenter.Measure(availableSize);

        _horizontalScrollingOn = oldMeasureSize.X > bounds.Width;
        _verticalScrollingOn = oldMeasureSize.Y > bounds.Height;

        if (_horizontalScrollingOn || _verticalScrollingOn)
        {
            var vsWidth = VerticalScrollbarWidth;
            var hsHeight = HorizontalScrollbarHeight;

            if (_horizontalScrollingOn && _isHorizontalScrollBarVisible.Value)
            {
                availableSize.Y -= hsHeight;
                if (availableSize.Y < 0)
                    availableSize.Y = 0;
            }

            if (_verticalScrollingOn && _isVerticalScrollBarVisible.Value)
            {
                availableSize.X -= vsWidth;
                if (availableSize.X < 0)
                    availableSize.X = 0;
            }

            // Remeasure with scrollbars
            var measureSize = ContentPresenter.Measure(availableSize);
            var bw = bounds.Width - (_verticalScrollingOn && _isVerticalScrollBarVisible.Value ? vsWidth : 0);
            var bh = bounds.Height - (_horizontalScrollingOn && _isHorizontalScrollBarVisible.Value ? hsHeight : 0);

            // Horizontal scrollbar frame & thumb
            _horizontalScrollbarFrame = new Rectangle(bounds.Left, bounds.Bottom - hsHeight, bw, hsHeight);
            var mw = Math.Max(1, measureSize.X); // verhindern Division durch 0
            _horizontalScrollbarThumb = new Rectangle(bounds.Left, bounds.Bottom - hsHeight,
                Math.Max(HorizontalScrollThumb.Value?.Size.X ?? 0, bw * bw / mw),
                HorizontalScrollThumb.Value?.Size.Y ?? 0);

            // Vertical scrollbar frame & thumb
            _verticalScrollbarFrame = new Rectangle(bounds.Left + bounds.Width - vsWidth, bounds.Top, vsWidth, bh);
            var mh = Math.Max(1, measureSize.Y); // verhindern Division durch 0
            _verticalScrollbarThumb = new Rectangle(bounds.Left + bounds.Width - vsWidth, bounds.Top,
                VerticalScrollThumb.Value?.Size.X ?? 0,
                Math.Max(VerticalScrollThumb.Value?.Size.Y ?? 0, bh * bh / mh));

            _thumbMaximumX = Math.Max(1, bw - _horizontalScrollbarThumb.Width);
            _thumbMaximumY = Math.Max(1, bh - _verticalScrollbarThumb.Height);

            // Update bounds für Presenter
            bounds.Width = _horizontalScrollingOn ? measureSize.X : availableSize.X;
            bounds.Height = _verticalScrollingOn ? measureSize.Y : availableSize.Y;
        }

        ContentPresenter.Arrange(bounds);

        // Fit scroll position in new maximums – nur wenn außerhalb
        var scrollMaximum = ScrollMaximum;
        var scrollPosition = ScrollPosition;
        var newScrollX = Math.Min(scrollPosition.X, scrollMaximum.X);
        var newScrollY = Math.Min(scrollPosition.Y, scrollMaximum.Y);

        if (newScrollX != scrollPosition.X || newScrollY != scrollPosition.Y)
        {
            ScrollPosition = new Point(newScrollX, newScrollY);
        }
    }


    public void ResetScroll()
    {
        ScrollPosition = Point.Zero;
    }

    private void DesktopTouchMoved(object? sender, EventArgs args)
    {
        if (!_startBoundsPos.HasValue || Desktop == null)
            return;

        var touchPosition = Desktop.TouchPosition.GetValueOrDefault();

        if (_scrollingBehavior == ScrollingBehavior.HorizontalThumb)
        {
            int delta = (_thumbMaximumX > 0)
                ? (touchPosition.X - _startBoundsPos.Value.X) * ScrollMaximum.X / _thumbMaximumX
                : 0;

            _startBoundsPos = new Point(touchPosition.X, 0);
            MoveThumb(delta, true, false);
        }
        else if (_scrollingBehavior == ScrollingBehavior.VerticalThumb)
        {
            int delta = (_thumbMaximumY > 0)
                ? (touchPosition.Y - _startBoundsPos.Value.Y) * ScrollMaximum.Y / _thumbMaximumY
                : 0;

            _startBoundsPos = new Point(0, touchPosition.Y);
            MoveThumb(delta, false, true);
        }
        else if (_scrollingBehavior == ScrollingBehavior.DirectDrag && CanDragScroll)
        {
            // ScrollPosition anpassen
            var scrollPosition = ScrollPosition;

            if (_canScrollHorizontal)
            {
                var deltaX = touchPosition.X - _startBoundsPos.Value.X;
                scrollPosition.X = MathHelper.Clamp(scrollPosition.X - deltaX, 0, ScrollMaximum.X);
            }

            if (_canScrollVertical)
            {
                var deltaY = touchPosition.Y - _startBoundsPos.Value.Y;
                scrollPosition.Y = MathHelper.Clamp(scrollPosition.Y - deltaY, 0, ScrollMaximum.Y);
            }

            ScrollPosition = scrollPosition;

            // Startpunkt für nächste Bewegung aktualisieren
            _startBoundsPos = touchPosition;
        }
    }

    private void DesktopTouchUp(object? sender, EventArgs args)
    {
        _startBoundsPos = null;
    }

    public override bool InputFallsThrough(Point localPos)
    {
        if (Background.Value != null)
        {
            return false;
        }

        if (_horizontalScrollingOn && _horizontalScrollbarFrame.Contains(localPos) ||
            _verticalScrollingOn && _verticalScrollbarFrame.Contains(localPos))
        {
            return false;
        }

        return true;
    }

    #region Style

    public override void ApplyFromStyle(Style? style)
    {
        base.ApplyFromStyle(style);

        style?.Apply(this, (target, sheet, property, value) =>
        {
            switch (property)
            {
                case nameof(HorizontalScrollBackground):
                    target.HorizontalScrollBackground = target.HorizontalScrollBackground.Override(sheet.FindRegion(value.RawValue));
                    break;
                case nameof(HorizontalScrollThumb):
                    target.HorizontalScrollThumb = target.HorizontalScrollThumb.Override(sheet.FindRegion(value.RawValue));
                    break;
                case nameof(VerticalScrollBackground):
                    target.VerticalScrollBackground = target.VerticalScrollBackground.Override(sheet.FindRegion(value.RawValue));
                    break;
                case nameof(VerticalScrollThumb):
                    target.VerticalScrollThumb = target.VerticalScrollThumb.Override(sheet.FindRegion(value.RawValue));
                    break;
            }
        });
    }

    public override void ApplyFrom(ElementBase other)
    {
        base.ApplyFrom(other);

        if (other is not ScrollViewer source)
            return;

        // Scrollbar-Texturen
        HorizontalScrollBackground = source.HorizontalScrollBackground;
        HorizontalScrollThumb = source.HorizontalScrollThumb;
        VerticalScrollBackground = source.VerticalScrollBackground;
        VerticalScrollThumb = source.VerticalScrollThumb;

        // Scroll-Einstellungen
        CanScrollHorizontal = source.CanScrollHorizontal;
        CanScrollVertical = source.CanScrollVertical;
        IsHorizontalScrollBarVisible = source.IsHorizontalScrollBarVisible;
        IsVerticalScrollBarVisible = source.IsVerticalScrollBarVisible;
        ScrollSpeed = source.ScrollSpeed;
        CanDragScroll = source.CanDragScroll;

        // Scrollposition übernehmen
        ScrollPosition = source.ScrollPosition;
    }

    protected override ElementBase CreateCloneInstance()
    {
        return new ScrollViewer();
    }

    #endregion
}