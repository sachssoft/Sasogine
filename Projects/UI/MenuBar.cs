using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// Horizontal commands and submenu entry points.
/// </summary>
public class MenuBar : Control
{
    private bool _useSkin = true;
    private static readonly System.ComponentModel.PropertyChangedEventArgs UseSkinArgs = new(nameof(UseSkin));
    /// <summary>
    /// Gets or sets whether atlas skin parts are used; false draws solid menu backgrounds and glyphs.
    /// </summary>
    public bool UseSkin { get => _useSkin; set => SetNotify(ref _useSkin, value, UseSkinArgs); }

    private IControlFont? _font;
    private int _active = -1;
    private ContextMenu? _popup;
    private int _popupIndex = -1;
    /// <summary>
    /// Provides the declared UI member.
    /// </summary>
    protected bool Vertical;
    /// <summary>
    /// Initializes a new menu bar instance.
    /// </summary>
    public MenuBar() : base()
    {
    }

    /// <summary>
    /// Gets items.
    /// </summary>
    public IList<MenuItem> Items => ConfigItems;
    /// <inheritdoc/>
    protected override string DefaultStyleName => "MenuBar";
    /// <inheritdoc/>
    protected override bool UseSkinFocus => UseSkin;
    private float RowHeight => ConfigItemHeight;

    private Bounds2 ItemBounds(int index)
    {
        float offset = 0;
        for (int i = 0; i < index; i++)
            offset += Vertical ? (Items[i].IsSeparator ? 8 : RowHeight) : (_font?.Measure(Items[i].Caption).Width ?? 0) + 28;
        return Vertical ? new Bounds2(Padding.Left, Padding.Top + offset, ContentBounds.Width, Items[index].IsSeparator ? 8 : RowHeight) : new Bounds2(Padding.Left + offset, Padding.Top, (_font?.Measure(Items[index].Caption).Width ?? 0) + 28, ContentBounds.Height);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        _font = GetFont(context);
        float width = 0, height = 0;
        foreach (var item in Items)
        {
            if (Vertical)
            {
                width = MathF.Max(width, _font.Measure(item.Caption).Width + _font.Measure(item.ShortcutText).Width + 80);
                height += item.IsSeparator ? 8 : RowHeight;
            }
            else
            {
                width += _font.Measure(item.Caption).Width + 28;
                height = MathF.Max(height, RowHeight);
            }
        }

        return new Size2(width, height);
    }

    private void Activate(int index)
    {
        if (index < 0 || index >= Items.Count || !Items[index].CanInvoke || Items[index].IsSeparator)
            return;
        _active = index;
        MenuItem item = Items[index];
        if (item.Items.Count == 0)
        {
            item.Execute();
            CloseMenuChain();
            return;
        }

        OpenSubmenu(index);
    }

    private void CloseSubmenu()
    {
        ContextMenu? old = _popup;
        _popup = null;
        _popupIndex = -1;
        if (old is not null)
            Workspace?.ClosePopup(old);
    }

    // Hover may navigate to a submenu, but it must never execute an item.
    private void OpenSubmenu(int index)
    {
        if (Workspace is not Workspace root || index < 0 || index >= Items.Count || !Items[index].CanInvoke || Items[index].Items.Count == 0)
            return;
        if (_popup is not null && _popupIndex == index)
            return;
        CloseSubmenu();
        MenuItem item = Items[index];
        var popup = new ContextMenu
        {
            UseSkin = UseSkin
        };
        if (!UseSkin)
        {
            popup.Background = Background;
            popup.HoverBackground = HoverBackground;
            popup.TextColor = TextColor;
            popup.DisabledTextColor = DisabledTextColor;
            popup.BorderColor = BorderColor;
            popup.BorderThickness = BorderThickness;
        }

        foreach (MenuItem child in item.Items)
            popup.Items.Add(child);
        Bounds2 local = ItemBounds(index);
        _popup = popup;
        _popupIndex = index;
        popup.ParentMenu = this;
        root.ShowPopup(popup, new Bounds2(Bounds.X + (Vertical ? local.Right : local.X), Bounds.Y + (Vertical ? local.Y : local.Bottom), 0, 0), this, () =>
        {
            if (_popup == popup)
            {
                _popup = null;
                _popupIndex = -1;
            }
        });
    }

    internal MenuBar? ParentMenu { get; set; }

    private void CloseMenuChain()
    {
        if (ParentMenu is MenuBar parent)
        {
            parent.CloseMenuChain();
            return;
        }

        if (this is ContextMenu)
            Workspace?.ClosePopup(this);
        else if (_popup is ContextMenu popup)
            Workspace?.ClosePopup(popup);
    }

    internal bool HandleShortcut(ControlInputEvent input)
    {
        bool Find(IList<MenuItem> items)
        {
            foreach (MenuItem item in items)
            {
                if (!item.CanInvoke)
                    continue;
                if (item.ShortcutKey != Keys.None && item.ShortcutKey == input.Key && item.ShortcutControl == input.Control && item.ShortcutAlt == input.Alt && item.ShortcutShift == input.Shift)
                {
                    item.Execute();
                    return true;
                }

                if (Find(item.Items))
                    return true;
            }

            return false;
        }

        if (Find(Items))
            return true;
        if (input.Alt)
            for (int i = 0; i < Items.Count; i++)
            {
                int marker = Items[i].Text.IndexOf('&');
                if (marker >= 0 && marker + 1 < Items[i].Text.Length && char.ToUpperInvariant(Items[i].Text[marker + 1]) == (char)(int)input.Key)
                {
                    Activate(i);
                    return true;
                }
            }

        return false;
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var e = args.Input;
        if (e.Type is ControlInputEventType.PointerMove or ControlInputEventType.PointerDown)
        {
            int hit = -1;
            for (int i = 0; i < Items.Count; i++)
            {
                Bounds2 b = ItemBounds(i);
                if (Contains(new Bounds2(Bounds.X + b.X, Bounds.Y + b.Y, b.Width, b.Height), e.Position.X, e.Position.Y))
                {
                    hit = i;
                    break;
                }
            }

            if (e.Type == ControlInputEventType.PointerDown && e.Button == MouseButton.Left)
            {
                Activate(hit);
                args.Handled = true;
            }
            else if (e.Type == ControlInputEventType.PointerMove && hit != _active)
            {
                bool navigate = Vertical || _popup is not null;
                _active = hit;
                if (navigate && hit >= 0)
                {
                    if (Items[hit].CanInvoke && Items[hit].Items.Count > 0)
                        OpenSubmenu(hit);
                    else
                        CloseSubmenu();
                }
            }

            return;
        }

        if (e.Type != ControlInputEventType.KeyDown)
            return;
        if (e.Key == Keys.Escape)
        {
            CloseMenuChain();
            args.Handled = true;
            return;
        }

        if (e.Key is Keys.Enter or Keys.Space || Vertical && e.Key == Keys.Right)
        {
            Activate(_active);
            args.Handled = true;
            return;
        }

        if (Vertical && e.Key == Keys.Left && ParentMenu is not null)
        {
            Workspace?.ClosePopup(this);
            args.Handled = true;
            return;
        }

        int direction = e.Key is Keys.Left or Keys.Up ? -1 : e.Key is Keys.Right or Keys.Down ? 1 : 0;
        if (direction == 0)
            return;
        for (int n = 0; n < Items.Count; n++)
        {
            _active = (_active + direction + Items.Count) % Items.Count;
            if (Items[_active].CanInvoke && !Items[_active].IsSeparator)
                break;
        }

        args.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        if (UseSkin)
            base.OnDraw(context);
        else
            context.FillRectangle(LocalBounds, Background);
        var font = GetFont(context);
        using (context.PushClipScope(new Bounds2(Padding.Left, Padding.Top, ContentBounds.Width, ContentBounds.Height)))
            for (int i = 0; i < Items.Count; i++)
            {
                MenuItem item = Items[i];
                Bounds2 b = ItemBounds(i);
                if (item.IsSeparator)
                {
                    var line = new Bounds2(b.X + 4, b.Y + 4, MathF.Max(0, b.Width - 8), 1);
                    if (UseSkin)
                        context.DrawSkinPart(this, "Separator", line);
                    else
                        context.FillRectangle(line, BorderColor);
                    continue;
                }

                if (UseSkin)
                    context.DrawSkinPart(this, "Item", b, !item.CanInvoke ? "Disabled" : i == _active ? "Hover" : "Normal");
                else if (item.CanInvoke && i == _active)
                    context.FillRectangle(b, HoverBackground);
                Color color = item.CanInvoke ? ConfigTextColor : ConfigDisabledTextColor;
                float textX = b.X + (Vertical ? 32 : 14);
                if (Vertical && item.IsChecked)
                {
                    if (UseSkin)
                        context.DrawSkinPart(this, "CheckGlyph", new Bounds2(b.X + 4, b.Y + (b.Height - 20) / 2, 20, 20));
                    else
                    {
                        var at = new Vector2(b.X + 8, b.Y + b.Height / 2);
                        context.DrawLine(at, at + new Vector2(4, 4), color, 2);
                        context.DrawLine(at + new Vector2(4, 4), at + new Vector2(11, -5), color, 2);
                    }
                }
                else if (Vertical && item.Icon is string icon)
                    context.DrawSkinRegion(icon, new Bounds2(b.X + 4, b.Y + (b.Height - 20) / 2, 20, 20));
                context.DrawText(font, item.Caption, new Vector2(textX, b.Y + (b.Height - font.Measure(item.Caption).Height) / 2), color);
                if (Vertical && item.Items.Count > 0)
                {
                    if (UseSkin)
                        context.DrawSkinPart(this, "SubmenuGlyph", new Bounds2(b.Right - 24, b.Y + (b.Height - 20) / 2, 20, 20));
                    else
                    {
                        var at = new Vector2(b.Right - 14, b.Y + b.Height / 2);
                        context.DrawLine(at + new Vector2(-3, -4), at + new Vector2(2, 0), color);
                        context.DrawLine(at + new Vector2(2, 0), at + new Vector2(-3, 4), color);
                    }
                }
                else if (Vertical && item.ShortcutText.Length > 0)
                    context.DrawText(font, item.ShortcutText, new Vector2(b.Right - font.Measure(item.ShortcutText).Width - 12, b.Y + (b.Height - font.Measure(item.ShortcutText).Height) / 2), ConfigDisabledTextColor);
            }
    }

    private StyleProperty<float> _styleItemHeight = new(30f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ItemHeightChangedArgs = new(nameof(ItemHeight));
    internal float ConfigItemHeight
    {
        get => _styleItemHeight.Value;
        set
        {
            if (_styleItemHeight.SetLocal(value))
                NotifyPropertyChanged(ItemHeightChangedArgs);
        }
    }

    /// <summary>
    /// Gets or sets ItemHeight; a local assignment overrides the style.
    /// </summary>
    public float ItemHeight { get => ConfigItemHeight; set => ConfigItemHeight = value; }

    private ElementCollection<MenuItem>? _ownedItems;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ItemsCollectionArgs = new("Items");
    internal IList<MenuItem> ConfigItems => _ownedItems ??= new(this, ItemsCollectionArgs);

    /// <summary>
    /// Applies this control's explicit style values without replacing local assignments.
    /// </summary>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "ItemHeight":
                    if (_styleItemHeight.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(ItemHeightChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "ItemHeight":
                return _styleItemHeight.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>
    /// Removes a local override and restores the current style value.
    /// </summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "ItemHeight":
                if (_styleItemHeight.ClearLocal())
                    NotifyPropertyChanged(ItemHeightChangedArgs);
                return;
            default:
                base.ResetStyleProperty(property);
                return;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
        if (_styleItemHeight.BeginStyle(30f))
            NotifyPropertyChanged(ItemHeightChangedArgs);
    }

    /// <summary>
    /// Commits final style values and notifies once per effective property change.
    /// </summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleItemHeight.CommitStyle())
            NotifyPropertyChanged(ItemHeightChangedArgs);
    }
}
