using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Surface.Controls;

public class Dialog : Window, IModalContent, IModalHost
{
    private StyleProperty<IBrush?> _modalBackground = new StyleProperty<IBrush?>(null, isUserSet: false);
    private StyleProperty<Keys> _confirmKey = new StyleProperty<Keys>(Keys.Enter, isUserSet: false);
    private StyleProperty<int> _confirmButtonIndex = new StyleProperty<int>(0, isUserSet: false);

    private readonly ObservableCollection<DialogButtonItem> _buttons = new ObservableCollection<DialogButtonItem>();
    private readonly HorizontalStackPanel _buttonsPanel = new HorizontalStackPanel();
    private readonly GridLayout _layout;
    private IModalHost? _modalHost;
    private Action<ModalResult> _modalResultAction;

    public Dialog()
    {
        ModalBackground = ColorUtils.ChangeAlphaChannel(Color.Black, 0.5f).ToBrush();

        _layout = (GridLayout)LayoutContainer!;
        _layout.RowsProportions.Add(new Proportion(ProportionType.Auto));

        _buttons.CollectionChanged += Buttons_CollectionChanged;

        _buttonsPanel.Spacing = 5;
        _buttonsPanel.HorizontalAlignment = Visuals.HorizontalAlignment.Center;
        _buttonsPanel.Padding = new Thickness(5);
        Grid.SetRow(_buttonsPanel, 2);

        Children.Add(_buttonsPanel);
    }

    public static Dialog Create(DialogButtons buttons)
    {
        var dialog = new Dialog();
        dialog.Buttons.Clear();
        switch (buttons)
        {
            case DialogButtons.OK:
                dialog.Buttons.Add(new DialogButtonItem("OK", 1));
                break;
            case DialogButtons.OKCancel:
                dialog.Buttons.Add(new DialogButtonItem("OK", 1));
                dialog.Buttons.Add(new DialogButtonItem("Cancel", 0));
                break;
            case DialogButtons.YesNo:
                dialog.Buttons.Add(new DialogButtonItem("Yes", 3));
                dialog.Buttons.Add(new DialogButtonItem("No", 2));
                break;
            case DialogButtons.YesNoCancel:
                dialog.Buttons.Add(new DialogButtonItem("Yes", 3));
                dialog.Buttons.Add(new DialogButtonItem("No", 2));
                dialog.Buttons.Add(new DialogButtonItem("Cancel", 0));
                break;
        }
        return dialog;
    }

    public static Dialog Create(DialogButtonItem[] buttons)
    {
        var dialog = new Dialog();
        dialog.Buttons.Clear();
        foreach (var button in buttons)
            dialog.Buttons.Add(button);
        return dialog;
    }

    #region Style Properties

    public StyleProperty<IBrush?> ModalBackground
    {
        get => _modalBackground;
        set => SetAndNotify(ref _modalBackground, value);
    }

    public StyleProperty<Keys> ConfirmKey
    {
        get => _confirmKey;
        set => SetAndNotify(ref _confirmKey, value);
    }

    public StyleProperty<int> ConfirmButtonIndex
    {
        get => _confirmButtonIndex;
        set => SetAndNotify(ref _confirmButtonIndex, value);
    }

    #endregion

    #region Direct Properties

    protected ObservableCollection<DialogButtonItem> Buttons => _buttons;

    #endregion

    IBrush? IModalContent.ModalBackground
    {
        get => _modalBackground.Value;
        set => _modalBackground = new StyleProperty<IBrush?>(value);
    }

    IEnumerable<IModalContent> IModalHost.Modals => throw new NotImplementedException();

    bool IModalContent.IsModalHosted => throw new NotImplementedException();

    private Button CreateDialogButton(DialogButtonItem item)
    {
        var btn = new Button
        {
            Width = 100,
            Height = 25,
            Content = item.Content,
            Tag = item
        };

        btn.Click += DialogButton_Click;
        return btn;
    }

    private void DialogButton_Click(object? sender, EventArgs e)
    {
        var button = sender as Button;
        var item = button?.Tag.Value as DialogButtonItem;

        if (item != null)
        {
            var result = new ModalResult(this, item.Result);
            _modalResultAction.Invoke(result);

            if (!result.Cancel)
                Close();
        }
    }

    public override void OnKeyDown(Keys k)
    {
        base.OnKeyDown(k);

        if (k == ConfirmKey.Value)
        {
            if (_confirmButtonIndex.Value >= 0 && _confirmButtonIndex.Value < _buttons.Count)
            {
                var buttonItem = _buttons[_confirmButtonIndex.Value];

                var result = new ModalResult(this, buttonItem.Result);
                _modalResultAction.Invoke(result);

                if (!result.Cancel)
                    Close();
            }
        }
    }

    protected internal virtual bool CanCloseByConfirm()
    {
        return true;
    }

    protected virtual void OnConfirmed() { }

    protected virtual void OnCancelled() { }


    #region Style

    public override void ApplyFromStyle(Style? style)
    {
        base.ApplyFromStyle(style);


        style?.Apply(this, (target, sheet, property, value) =>
        {
            switch (property)
            {
                case nameof(ConfirmKey):
                    target.ConfirmKey = target.ConfirmKey.Override(value.ConvertToEnum<Keys>());
                    break;

                case nameof(ModalBackground):
                    target.ModalBackground = target.ModalBackground.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                    break;

                case nameof(ConfirmButtonIndex):
                    target.ConfirmButtonIndex = target.ConfirmButtonIndex.Override(value.ConvertTo<int>());
                    break;
            }
        });
    }

    public override void ApplyFrom(ElementBase other)
    {
        base.ApplyFrom(other);

        if (other is not Dialog source)
            return;

        ConfirmKey = source.ConfirmKey;
        ModalBackground = source.ModalBackground;
        ConfirmButtonIndex = source.ConfirmButtonIndex;
    }

    protected override ElementBase CreateCloneInstance()
    {
        return new Dialog();
    }

    #endregion

    public override void Show(IWindowHost host)
    {
        if (_modalHost != null)
            throw new InvalidOperationException("Window is already shown.");

        base.Show(host);
    }

    public void ShowModal(IModalHost host)
    {
        ShowModal(host, result => { });
    }

    public virtual void ShowModal(IModalHost host, Action<ModalResult> result)
    {
        if (IsWindowHosted || _modalHost != null)
            throw new InvalidOperationException("Window is already shown.");

        IsVisible = true;
        host.AddModal(this);
        _modalHost = host;
        _modalResultAction = result;

        InvalidateArrange();
    }

    protected override void InternalArrange()
    {
        base.InternalArrange();

        if (_modalHost != null)
            SetPositionCenter(_modalHost.Bounds);
    }

    public Task<ModalResult> ShowModalAsync(IModalHost host)
    {
        var tcs = new TaskCompletionSource<ModalResult>();

        // Modal anzeigen und Ergebnis setzen
        ShowModal(host, result => tcs.SetResult(result));

        return tcs.Task;
    }

    public override void Close()
    {
        if (_modalHost != null)
        {
            var host = (IModalHost)_modalHost;
            host.RemoveModal(this);
            _modalHost = null;
            IsVisible = false;
        }
        else
        {
            base.Close();
        }
    }

    private void Buttons_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (DialogButtonItem item in e.NewItems)
            {
                var btn = CreateDialogButton(item);
                _buttonsPanel.Widgets.Add(btn);
            }
        }

        if (e.OldItems != null)
        {
            foreach (DialogButtonItem item in e.OldItems)
            {
                var btn = _buttonsPanel.Widgets
                            .OfType<Button>()
                            .FirstOrDefault(w => w.Tag == item);

                if (btn != null) _buttonsPanel.Widgets.Remove(btn);
            }
        }
    }

    void IModalHost.AddModal(IModalContent modal)
    {
        if (modal is Widget w && !Children.Contains(w))
            Children.Add(w);
    }

    void IModalHost.RemoveModal(IModalContent modal)
    {
        if (modal is Widget w && Children.Contains(w))
            Children.Remove(w);
    }

    void IModalContent.Close()
    {
        Close();
    }

    void IModalContent.ShowModal(IModalHost host, Action<ModalResult> result)
    {
        ShowModal(host, result);
    }

    Task<ModalResult> IModalContent.ShowModalAsync(IModalHost host)
    {
        return ShowModalAsync(host);
    }
}
