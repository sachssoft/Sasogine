using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System;

namespace Sachssoft.Sasogine.Surface.Controls;

public class ContentControl : Widget
{
    private StyleProperty<object?> _content = new StyleProperty<object?>(null, isUserSet: false);
    private StyleProperty<VerticalAlignment> _contentVerticalAlignment = new StyleProperty<VerticalAlignment>(Visuals.VerticalAlignment.Stretch, isUserSet: false);
    private StyleProperty<HorizontalAlignment> _contentHorizontalAlignment = new StyleProperty<HorizontalAlignment>(Visuals.HorizontalAlignment.Stretch, isUserSet: false);
    private Widget? _contentPresenter;
    private readonly SingleItemLayout<Widget> _layout;

    public ContentControl()
    {
        _layout = new SingleItemLayout<Widget>(this);
        LayoutContainer = _layout;
    }

    #region Style Properties

    public StyleProperty<object?> Content
    {
        get => _content;
        set
        {
            OnContentChanging(EventArgs.Empty);
            var oldPresenter = _contentPresenter;

            if (SetAndNotify(ref _content, value))
            {
                // Alten Presenter trennen, falls vorhanden
                if (_contentPresenter != null)
                {
                    if (_contentPresenter.Parent == this)
                    {
                        _contentPresenter.Parent = null;
                    }
                    else if (_contentPresenter.Parent != null)
                    {
                        throw new InvalidOperationException("ContentPresenter belongs to another parent");
                    }
                }

                // Neuen Presenter erstellen
                _contentPresenter = PresenterCreated(_content.Value);

                // Parent korrekt setzen
                if (_contentPresenter != null)
                    _contentPresenter.Parent = this;

                // Layout aktualisieren
                OnLayoutChanged(new PresenterEventArgs(oldPresenter, _contentPresenter));
                OnContentChanged(EventArgs.Empty);
            }
        }
    }

    public StyleProperty<HorizontalAlignment> ContentHorizontalAlignment
    {
        get
        {
            if (_contentPresenter == null)
                return _contentHorizontalAlignment;

            if (_contentPresenter.HorizontalAlignment != _contentHorizontalAlignment.Value)
                _contentPresenter.HorizontalAlignment = _contentHorizontalAlignment.Value;

            return _contentHorizontalAlignment;
        }
        set
        {
            if (SetAndNotify(ref _contentHorizontalAlignment, value))
            {
                if (_contentPresenter != null)
                    _contentPresenter.HorizontalAlignment = value;
            }
        }
    }

    public StyleProperty<VerticalAlignment> ContentVerticalAlignment
    {
        get
        {
            if (_contentPresenter == null)
                return _contentVerticalAlignment;

            if (_contentPresenter.VerticalAlignment != _contentVerticalAlignment.Value)
                _contentPresenter.VerticalAlignment = _contentVerticalAlignment.Value;

            return _contentVerticalAlignment;
        }
        set
        {
            if (SetAndNotify(ref _contentVerticalAlignment, value))
            {
                if (_contentPresenter != null)
                    _contentPresenter.VerticalAlignment = value;
            }
        }
    }

    #endregion

    #region Direct Properties

    public Widget? ContentPresenter
    {
        get => _contentPresenter;
    }

    #endregion

    #region Style

    public override void ApplyFromStyle(Style? style)
    {
        base.ApplyFromStyle(style);

        if (ContentPresenter is Label label)
        {
            var contentLabelStyle = style?.FindStyle(typeof(Label), "content");
            label.ApplyFromStyle(contentLabelStyle);
        }
        else if (ContentPresenter is Image image)
        {
            var contentImageStyle = style?.FindStyle(typeof(Image), "content");
            image.ApplyFromStyle(contentImageStyle);
        }

        style?.Apply(this, (target, sheet, property, value) =>
        {
            switch (property)
            {
                case nameof(ContentHorizontalAlignment):
                    target.ContentHorizontalAlignment = target.ContentHorizontalAlignment.Override(value.ConvertToEnum<HorizontalAlignment>());
                    break;
                case nameof(ContentVerticalAlignment):
                    target.ContentVerticalAlignment = target.ContentVerticalAlignment.Override(value.ConvertToEnum<VerticalAlignment>());
                    break;
            }
        });
    }

    public override void ApplyFrom(ElementBase other)
    {
        base.ApplyFrom(other);

        if (other is not ContentControl source)
            return;

        if (source.Content.Value is ICloneable cloneable)
        {
            Content = cloneable.Clone();
            return;
        }

        Content = source.Content.Value;
        ContentHorizontalAlignment = source.ContentHorizontalAlignment;
        ContentVerticalAlignment = source.ContentVerticalAlignment;
    }

    protected override ElementBase CreateCloneInstance()
    {
        return new ContentControl();
    }

    #endregion

    public override IBrush? GetCurrentBackground()
    {
        return Background.Value;
    }

    protected virtual Widget? PresenterCreated(object? obj)
    {
        if (_content.Value is null)
        {
            return null;
        }
        else if (_content.Value is Widget w)
        {
            w.HorizontalAlignment = _contentHorizontalAlignment;
            w.VerticalAlignment = _contentVerticalAlignment;
            return w;
        }
        else if (_content.Value is ITextureRegion textureRegion)
        {
            return new Image()
            {
                HorizontalAlignment = Visuals.HorizontalAlignment.Center,
                VerticalAlignment = Visuals.VerticalAlignment.Center,
                Width = textureRegion.Size.X,
                Height = textureRegion.Size.Y,
                Visual = new StyleProperty<ITextureRegion?>(textureRegion),
                HoveredVisual = new StyleProperty<ITextureRegion?>(textureRegion)
            };
        }
        else if (_content.Value is IBrush brush)
        {
            return new Widget()
            {
                HorizontalAlignment = _contentHorizontalAlignment,
                VerticalAlignment = _contentVerticalAlignment,
                Background = new StyleProperty<IBrush?>(brush)
            };
        }
        else
        {
            var container = new Grid()
            {
                HorizontalAlignment = _contentHorizontalAlignment,
                VerticalAlignment = _contentVerticalAlignment,
            };
            var label = new Label()
            {
                HorizontalAlignment = Visuals.HorizontalAlignment.Center,
                VerticalAlignment = Visuals.VerticalAlignment.Center,
                Text = obj?.ToString()
            };
            container.Children.Add(label);
            return container;
        }
    }

    protected virtual void OnLayoutChanged(PresenterEventArgs e)
    {
        _layout.Child = e.NewPresenter;
    }

    protected void UpdateLayout()
    {
        var oldPresenter = _contentPresenter;
        _contentPresenter = PresenterCreated(_content.Value);
        OnLayoutChanged(new PresenterEventArgs(oldPresenter, _contentPresenter));
    }

    protected virtual void OnContentChanging(EventArgs e) { }

    protected virtual void OnContentChanged(EventArgs e) { }
}
