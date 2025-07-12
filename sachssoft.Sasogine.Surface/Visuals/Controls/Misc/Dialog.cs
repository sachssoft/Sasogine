using Microsoft.Xna.Framework.Input;
using System;
using System.ComponentModel;
using System.Xml.Serialization;
using sachssoft.Sasogine.Surface.Attributes;
using sachssoft.Sasogine.Surface.Visuals.Styles;

namespace sachssoft.Sasogine.Surface.Visuals.Controls;

[StyleTypeName("Window")]
public class Dialog : Window
{
    [Browsable(false)]
    [XmlIgnore]
    // By Tobias Sachs
    [Obsolete("Use ButtonConfirm Instead")]
    public Button ButtonOk => ButtonConfirm;

    [Browsable(false)]
    [XmlIgnore]
    public Button ButtonConfirm { get; private set; }

    [Browsable(false)]
    [XmlIgnore]
    public Button ButtonCancel { get; private set; }

    public Action<Dialog>? Confirmed { get; set; } // Neu von Tobias Sachs!

    [Category("Behavior")]
    [DefaultValue(Keys.Enter)]
    public Keys ConfirmKey { get; set; }

    private HorizontalStackPanel _buttons_panel;

    // By Tobias Sachs
    [Obsolete("Should Remove")]
    [Browsable(false)]
    public bool ShowOKButton
    {
        get => ButtonOk.IsVisible;
        set => ButtonOk.IsVisible = value;
    }

    // By Tobias Sachs
    [Obsolete("Should Remove")]
    [Browsable(false)]
    public bool ShowCancelButton
    {
        get => ButtonCancel.IsVisible;
        set => ButtonCancel.IsVisible = value;
    }

    public Dialog(string styleName = Stylesheet.DefaultStyleName) : base(styleName)
    {
        ConfirmKey = Keys.Enter;

        _buttons_panel = new HorizontalStackPanel()
        {
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        ButtonConfirm = new Button
        {
            Width = 100,
            Height = 25,
            Content = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = "OK",
                TextAlignment = FontStashSharp.RichText.TextHorizontalAlignment.Center
            }
        };

        ButtonConfirm.Click += (sender, args) =>
        {
            OnConfirmed();
        };

        _buttons_panel.Widgets.Add(ButtonConfirm);

        ButtonCancel = new Button
        {
            Width = 100,
            Height = 25,
            Content = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = "Cancel",
                TextAlignment = FontStashSharp.RichText.TextHorizontalAlignment.Center
            }
        };

        ButtonCancel.Click += (sender, args) =>
        {
            Result = false;
            Close();
        };

        _buttons_panel.Widgets.Add(ButtonCancel);
        Children.Add(_buttons_panel);
    }

    protected void AddButton(string text, Action? click = null)
    {
        var button = new Button
        {
            Width = 100,
            Height = 25,
            Content = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = text,
                TextAlignment = FontStashSharp.RichText.TextHorizontalAlignment.Center
            }
        };
        button.Click += (s, e) => click?.Invoke();
        _buttons_panel.Widgets.Add(button);
    }

    public override void OnKeyDown(Keys k)
    {
        FireKeyDown(k);

        if (k == CloseKey)
        {
            CloseButton.DoClick();
        }
        else if (k == ConfirmKey)
        {
            ButtonConfirm.DoClick();
        }
    }

    // By Tobias Sachs
    [Obsolete("Use Confirmed Instead")]
    protected internal virtual void OnOk()
    {
        OnConfirmed();
    }

    protected internal virtual void OnConfirmed()
    {
        if (!CanCloseByConfirm())
        {
            return;
        }

        Result = true;
        Close();

        // Neu
        Confirmed?.Invoke(this);
    }

    // By Tobias Sachs
    [Obsolete("Use CanCloseByConfirm Instead")]
    protected internal virtual bool CanCloseByOk()
    {
        return CanCloseByConfirm();
    }

    protected internal virtual bool CanCloseByConfirm()
    {
        return true;
    }

    protected internal override void CopyFrom(Widget w)
    {
        base.CopyFrom(w);

        var dialog = (Dialog)w;
        ConfirmKey = dialog.ConfirmKey;
    }

    public static Dialog CreateMessageBox(string title, Widget content)
    {
        var w = new Dialog
        {
            Title = title,
            Content = content
        };

        return w;
    }

    public static Dialog CreateMessageBox(string title, string message)
    {
        var messageLabel = new Label
        {
            Text = message,
            Wrap = true
        };

        return CreateMessageBox(title, messageLabel);
    }
}