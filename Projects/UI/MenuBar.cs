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
    /// <summary>Caches the command row height notification.</summary>
    private static readonly System.ComponentModel.PropertyChangedEventArgs ItemHeightChangedArgs = new(nameof(ItemHeight));

    /// <summary>Caches the entry collection notification.</summary>
    private static readonly System.ComponentModel.PropertyChangedEventArgs ItemsCollectionArgs = new("Items");

    /// <summary>Caches the atlas drawing mode notification.</summary>
    private static readonly System.ComponentModel.PropertyChangedEventArgs UseSkinArgs = new(nameof(UseSkin));

    /// <summary>Stores the currently open child popup.</summary>
    private ContextMenu? _popup;

    /// <summary>Lazily stores menu entries with owner change notifications.</summary>
    private ElementCollection<MenuEntry>? _ownedItems;

    /// <summary>Caches the font used to measure entry captions.</summary>
    private IControlFont? _font;

    /// <summary>Stores the styled command row height and optional local override.</summary>
    private StyleProperty<float> _styleItemHeight = new(30f);

    /// <summary>
    /// Selects vertical popup layout instead of horizontal menu bar layout.
    /// </summary>
    protected bool Vertical;

    /// <summary>Stores whether the menu draws its atlas parts.</summary>
    private bool _useSkin = true;

    /// <summary>Stores the active command index, or minus one.</summary>
    private int _active = -1;

    /// <summary>Stores the command index associated with the child popup.</summary>
    private int _popupIndex = -1;

    /// <summary>
    /// Initializes a new menu bar instance.
    /// </summary>
    public MenuBar() : base()
    {
    }

    /// <summary>
    /// Gets or sets whether atlas skin parts are used; false draws solid menu backgrounds and glyphs.
    /// </summary>
    public bool UseSkin { get => _useSkin; set => SetNotify(ref _useSkin, value, UseSkinArgs); }

    /// <summary>
    /// Gets the command and separator entries displayed by this menu.
    /// </summary>
    public IList<MenuEntry> Items => ConfigItems;

    /// <inheritdoc/>
    protected override string DefaultStyleName => "MenuBar";

    /// <inheritdoc/>
    protected override bool UseSkinFocus => UseSkin;

    /// <summary>Gets the effective command row height.</summary>
    private float RowHeight => ConfigItemHeight;

    /// <summary>Gets or sets the containing menu for closing a popup chain; null identifies a top-level menu.</summary>
    internal MenuBar? ParentMenu { get; set; }

    /// <summary>Gets or sets the effective styled command row height.</summary>
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

    /// <summary>Gets the lazily created entry collection that invalidates this menu on changes.</summary>
    internal IList<MenuEntry> ConfigItems => _ownedItems ??= new(this, ItemsCollectionArgs);

    /// <summary>Gets the main-axis space reserved by an entry.</summary>
    /// <param name = "entry">The command or separator being laid out.</param>
    /// <returns>Eight logical pixels for separators, otherwise the row height or caption width.</returns>
    private float EntryLength(MenuEntry entry) => entry is MenuItem item ? Vertical ? RowHeight : (_font?.Measure(item.Caption).Width ?? 0) + 28 : 8;

    /// <summary>Gets an entry's local bounds including separators in its preceding offset.</summary>
    /// <param name = "index">The entry index.</param>
    /// <returns>The entry's reserved local slot.</returns>
    private Bounds2 ItemBounds(int index)
    {
        float offset = 0;
        for (int i = 0; i < index; i++)
            offset += EntryLength(Items[i]);
        float length = EntryLength(Items[index]);
        return Vertical ? new Bounds2(Padding.Left, Padding.Top + offset, ContentBounds.Width, length) : new Bounds2(Padding.Left + offset, Padding.Top, length, ContentBounds.Height);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        _font = GetFont(context);
        float width = 0, height = 0;
        foreach (MenuEntry entry in Items)
        {
            if (Vertical)
            {
                if (entry is MenuItem item)
                    width = MathF.Max(width, _font.Measure(item.Caption).Width + _font.Measure(item.ShortcutText).Width + 80);
                height += EntryLength(entry);
            }
            else
            {
                width += EntryLength(entry);
                height = MathF.Max(height, RowHeight);
            }
        }

        return new Size2(width, height);
    }

    /// <summary>Invokes a command or opens its submenu after availability checks.</summary>
    /// <param name = "index">The entry index to activate.</param>
    private void Activate(int index)
    {
        if (index < 0 || index >= Items.Count || Items[index] is not MenuItem item || !item.CanInvoke)
            return;
        _active = index;
        if (item.Items.Count == 0)
        {
            item.Execute();
            CloseMenuChain();
            return;
        }

        OpenSubmenu(index);
    }

    /// <summary>Closes and releases the current child popup.</summary>
    private void CloseSubmenu()
    {
        ContextMenu? old = _popup;
        _popup = null;
        _popupIndex = -1;
        if (old is not null)
            Workspace?.ClosePopup(old);
    }

    // Hover may navigate to a submenu, but it must never execute an item.

    /// <summary>Opens a command's mixed entry collection without executing the command.</summary>
    /// <param name = "index">The command index whose submenu should open.</param>
    private void OpenSubmenu(int index)
    {
        if (Workspace is not Workspace root || index < 0 || index >= Items.Count || Items[index] is not MenuItem item || !item.CanInvoke || item.Items.Count == 0)
            return;
        if (_popup is not null && _popupIndex == index)
            return;
        CloseSubmenu();
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

        foreach (MenuEntry child in item.Items)
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

    /// <summary>Closes popups through the topmost menu in the current chain.</summary>
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

    /// <summary>Finds a command shortcut or mnemonic while ignoring non-command entries.</summary>
    /// <param name = "input">The keyboard event to match.</param>
    /// <returns>Whether a matching shortcut or mnemonic was handled.</returns>
    internal bool HandleShortcut(ControlInputEvent input)
    {
        bool Find(IList<MenuEntry> items)
        {
            foreach (MenuEntry entry in items)
            {
                if (entry is not MenuItem item || !item.CanInvoke)
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
                if (Items[i] is not MenuItem item)
                    continue;
                int marker = item.Text.IndexOf('&');
                if (marker >= 0 && marker + 1 < item.Text.Length && char.ToUpperInvariant(item.Text[marker + 1]) == (char)(int)input.Key)
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
                    if (Items[hit] is MenuItem item && item.CanInvoke && item.Items.Count > 0)
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
        int candidate = _active;
        _active = -1;
        for (int n = 0; n < Items.Count; n++)
        {
            candidate = (candidate + direction + Items.Count) % Items.Count;
            if (Items[candidate] is MenuItem item && item.CanInvoke)
            {
                _active = candidate;
                break;
            }
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
                Bounds2 b = ItemBounds(i);
                if (Items[i] is not MenuItem item)
                {
                    var line = Vertical ? new Bounds2(b.X + 4, b.Y + 4, MathF.Max(0, b.Width - 8), 1) : new Bounds2(b.X + 4, b.Y + 4, 1, MathF.Max(0, b.Height - 8));
                    if (!UseSkin || !context.DrawSkinPart(this, "Separator", line))
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

    /// <inheritdoc/>
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
