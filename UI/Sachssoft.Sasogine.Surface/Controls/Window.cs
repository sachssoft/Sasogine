using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Presentation;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Behaviors;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Utility;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Surface.Controls;

public class Window : ContentControl, IWindowContent, IDisposable
{
    private StyleProperty<string?> _title = new StyleProperty<string?>(null, isUserSet: false);
    private StyleProperty<Color> _titleColor = new StyleProperty<Color>(Color.White, isUserSet: false);
    private StyleProperty<Font?> _titleFont = new StyleProperty<Font?>(null, isUserSet: false);
    private StyleProperty<bool> _isCloseButtonVisible = new StyleProperty<bool>(true, isUserSet: false);
    private StyleProperty<bool> _isClosable = new StyleProperty<bool>(true, isUserSet: false);
    private StyleProperty<bool> _isMovable = new StyleProperty<bool>(true, isUserSet: false);
    private StyleProperty<bool> _isSizable = new StyleProperty<bool>(false, isUserSet: false);
    private StyleProperty<Keys> _closeKey = new StyleProperty<Keys>(Keys.Escape, isUserSet: false);
    private StyleProperty<IBrush?> _activeBackground = new StyleProperty<IBrush?>(null, isUserSet: false);
    private StyleProperty<IBrush?> _inactiveBackground = new StyleProperty<IBrush?>(null, isUserSet: false);

    private readonly GridLayout _layout = new GridLayout();
    private readonly Label _titleLabel;
    private readonly HorizontalStackPanel _titleContainer = new HorizontalStackPanel();
    private readonly Button _closeButton = new Button();
    private readonly Image _closeButtonIcon = new Image();

    private IWindowHost? _windowHost;

    #region Events

    public event EventHandler<CancellableEventArgs> Closing;
    public event EventHandler Closed;

    #endregion

    public Window()
    {
        VerticalAlignment = Visuals.VerticalAlignment.Top;
        HorizontalAlignment = Visuals.HorizontalAlignment.Left;

        _layout.RowsProportions.Add(new Proportion(ProportionType.Auto));
        _layout.RowsProportions.Add(new Proportion(ProportionType.Fill));
        _layout.RowSpacing = 5;
        LayoutContainer = _layout;

        AcceptsKeyboardFocus = true;
        DragDirection = Controls.DragDirection.Both;

        _titleContainer.Spacing = 5;
        DragHandle = _titleContainer;

        _titleLabel = new Label();
        StackPanel.SetProportionType(_titleLabel, ProportionType.Fill);
        _titleContainer.Widgets.Add(_titleLabel);

        _closeButton.Content = _closeButtonIcon;
        CloseButton.Click += CloseButton_Click;
        _titleContainer.Widgets.Add(CloseButton);
        Grid.SetRow(_titleContainer, 0);

        Children.Add(_titleContainer);
    }

    #region Style Properties

    public StyleProperty<string?> Title
    {
        get => _title;
        set
        {
            if (SetAndNotify(ref _title, value))
            {
                UpdateWindowLayout();
            }
        }
    }

    public StyleProperty<Color> TitleColor
    {
        get => _titleColor;
        set
        {
            if (SetAndNotify(ref _titleColor, value))
            {
                UpdateWindowLayout();
            }
        }
    }

    public StyleProperty<Font?> TitleFont
    {
        get => _titleFont;
        set
        {
            if (SetAndNotify(ref _titleFont, value))
            {
                UpdateWindowLayout();
            }
        }
    }

    public StyleProperty<bool> IsCloseButtonVisible
    {
        get => _isCloseButtonVisible;
        set
        {
            if (SetAndNotify(ref _isCloseButtonVisible, value))
            {
                UpdateWindowLayout();
            }
        }
    }

    public StyleProperty<bool> IsClosable
    {
        get => _isClosable;
        set
        {
            if (SetAndNotify(ref _isClosable, value))
            {
                UpdateWindowLayout();
            }
        }
    }

    public StyleProperty<bool> IsMovable
    {
        get => _isMovable;
        set
        {
            if (SetAndNotify(ref _isMovable, value))
            {
                UpdateWindowLayout();
            }
        }
    }

    public StyleProperty<bool> IsSizable
    {
        get => _isSizable;
        set => SetAndNotify(ref _isSizable, value);
    }

    public StyleProperty<Keys> CloseKey
    {
        get => _closeKey;
        set => SetAndNotify(ref _closeKey, value);
    }

    public StyleProperty<IBrush?> ActiveBackground
    {
        get => _activeBackground;
        set => SetAndNotify(ref _activeBackground, value);
    }

    public StyleProperty<IBrush?> InactiveBackground
    {
        get => _inactiveBackground;
        set => SetAndNotify(ref _inactiveBackground, value);
    }

    #endregion

    #region Direct Properties

    protected Container TitleContainer => _titleContainer;

    protected Button CloseButton => _closeButton;

    public bool IsWindowHosted => _windowHost != null;

    public bool IsActive { get; internal set; } = true; // Später implementieren

    #endregion

    private void UpdateWindowLayout()
    {
        _titleLabel.Text = _title.Value;
        _titleLabel.TextColor = _titleColor.Value;
        _titleLabel.Font = _titleFont.Value ?? DefaultStyle.TitleFont;
        _closeButton.IsVisible = _isCloseButtonVisible.Value;
        _closeButton.IsEnabled = _isClosable.Value;

        DragDirection = _isMovable.Value ? Controls.DragDirection.Both : Controls.DragDirection.None;
    }

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        switch (propertyName)
        {
            case nameof(DragDirection):
                if (DragDirection == Controls.DragDirection.None)
                {
                    _isMovable = new StyleProperty<bool>(false);
                }
                else
                {
                    if (DragDirection == Controls.DragDirection.Horizontal ||
                        DragDirection == Controls.DragDirection.Vertical)
                    {
                        using (SuppressNotifications())
                        {
                            DragDirection = Controls.DragDirection.Both;
                        }
                    }

                    _isMovable = new StyleProperty<bool>(true);
                }
                break;
        }
    }

    protected override void OnLayoutChanged(PresenterEventArgs e)
    {
        if (e.OldPresenter != null)
        {
            Children.Remove(e.OldPresenter);
        }

        if (e.NewPresenter != null)
        {
            Grid.SetRow(e.NewPresenter, 1);
            Children.Add(e.NewPresenter);
        }
    }

    protected override void InternalArrange()
    {
        base.InternalArrange();

        if (_windowHost != null)
            SetPositionCenter(_windowHost.Bounds);
    }

    protected void SetPositionCenter(Rectangle hostBounds)
    {
        var size = Bounds.Size();
        Left = (hostBounds.Width / 2 - size.X / 2);
        Top = (hostBounds.Height / 2 - size.Y / 2);
    }

    internal protected override void OnTouchDown()
    {
        BringToFront();
        base.OnTouchDown();
    }

    public override void OnKeyDown(Keys k)
    {
        base.OnKeyDown(k);

        if (k == CloseKey)
        {
            Close();
        }
    }

    public virtual void Show(IWindowHost host)
    {
        if (_windowHost != null)
            throw new InvalidOperationException("Window is already shown.");

        IsVisible = true;
        host.AddWindow(this);
        _windowHost = host;

        InvalidateArrange();
    }

    public void Show(IWindowHost host, bool centerPosition)
    {
        Show(host);

        if (centerPosition)
            SetPositionCenter(host.Bounds);
    }

    public virtual Task ShowAsync(IWindowHost host)
    {
        Show(host);
        return Task.CompletedTask;
    }

    public Task ShowAsync(IWindowHost host, bool centerPosition)
    {
        var task = ShowAsync(host);

        if (centerPosition)
            SetPositionCenter(host.Bounds);

        return task;
    }

    public virtual void Close()
    {
        if (_windowHost == null)
            return;

        _windowHost.RemoveWindow(this);
        IsVisible = false;
    }

    private void CloseButton_Click(object? sender, EventArgs e)
    {
        Close();
    }

    #region Style

    public override void ApplyFromStyle(Style? style)
    {
        base.ApplyFromStyle(style);

        var titleStyle = style?.FindStyle(typeof(Label), "Title");
        _titleLabel.ApplyFromStyle(titleStyle);

        var closeButtonStyle = style?.FindStyle(typeof(Button), "CloseButton");
        CloseButton.ApplyFromStyle(closeButtonStyle);

        var closeButtonIconStyle = closeButtonStyle?.FindStyle(typeof(Image), "Icon");
        _closeButtonIcon.ApplyFromStyle(closeButtonIconStyle);

        style?.Apply(this, (target, sheet, property, value) =>
        {
            switch (property)
            {
                case nameof(Title):
                    target.Title = target.Title.Override(value.RawValue);
                    break;
                case nameof(TitleColor):
                    target.TitleColor = target.TitleColor.Override(value.ConvertTo<Color>());
                    break;
                case nameof(TitleFont):
                    target.TitleFont = target.TitleFont.Override(sheet.GetFont(value.RawValue));
                    break;
                case nameof(IsCloseButtonVisible):
                    target.IsCloseButtonVisible = target.IsCloseButtonVisible.Override(value.ConvertTo<bool>());
                    break;
                case nameof(IsClosable):
                    target.IsClosable = target.IsClosable.Override(value.ConvertTo<bool>());
                    break;
                case nameof(IsMovable):
                    target.IsMovable = target.IsMovable.Override(value.ConvertTo<bool>());
                    break;
                case nameof(IsSizable):
                    target.IsSizable = target.IsSizable.Override(value.ConvertTo<bool>());
                    break;
                case nameof(CloseKey):
                    target.CloseKey = target.CloseKey.Override(value.ConvertToEnum<Keys>());
                    break;
                case nameof(ActiveBackground):
                    target.ActiveBackground = target.ActiveBackground.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                    break;
                case nameof(InactiveBackground):
                    target.InactiveBackground = target.InactiveBackground.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                    break;
            }
        });
    }

    public override void ApplyFrom(ElementBase other)
    {
        base.ApplyFrom(other);

        if (other is not Window source)
            return;

        Title = source.Title;
        TitleColor = source.TitleColor;
        TitleFont = source.TitleFont;
        IsCloseButtonVisible = source.IsCloseButtonVisible;
        IsClosable = source.IsClosable;
        IsMovable = source.IsMovable;
        IsSizable = source.IsSizable;
        CloseKey = source.CloseKey;
        ActiveBackground = source.ActiveBackground;
        InactiveBackground = source.InactiveBackground;
    }

    protected override ElementBase CreateCloneInstance()
    {
        return new Window();
    }

    public override IBrush? GetCurrentBackground()
    {
        if (!IsActive)
            return InactiveBackground.Value ?? Background.Value;

        return ActiveBackground.Value ?? Background.Value;
    }

    #endregion

    protected virtual void OnClosing(CancellableEventArgs e)
    {
        Closing?.Invoke(this, e);
    }

    protected virtual void OnClosed(EventArgs e)
    {
        Closed?.Invoke(this, e);
    }

    public virtual void Dispose()
    {
        CloseButton.Click -= CloseButton_Click;
    }
}