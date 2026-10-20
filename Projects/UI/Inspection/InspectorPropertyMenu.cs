using System.ComponentModel;

namespace Sachssoft.Engine.Controls.Inspection;

/// <summary>Opens the trailing property menu without polling values or allocating during unchanged updates.</summary>
public sealed class InspectorPropertyMenu : Button
{
    private readonly InspectorView _inspector;
    private readonly Control _editor;
    private readonly IInspectorPropertyActions? _actions;
    private readonly ContextMenu _menu = new()
    {
        UseSkin = false,
        Background = new Color(28, 31, 38),
        HoverBackground = new Color(53, 59, 70),
        TextColor = new Color(230, 233, 239),
        DisabledTextColor = new Color(115, 121, 132),
        BorderColor = new Color(65, 71, 83),
        BorderThickness = 1
    };
    private readonly MenuItem _reset = new() { Text = "Reset" }, _copy = new() { Text = "Copy value" }, _paste = new() { Text = "Paste value" };
    private bool _readOnly;
    private Action? _resetAction;
    private static readonly PropertyChangedEventArgs ResetActionArgs = new(nameof(ResetAction));
    internal InspectorPropertyMenu(InspectorView inspector, Control editor, IInspectorPropertyActions? actions)
    {
        _inspector = inspector; _editor = editor; _actions = actions;

        ToolTip = "Property actions";
        _reset.Action = ResetValue; _copy.Action = CopyValue; _paste.Action = PasteValue;
        _menu.Items.Add(_reset); _menu.Items.Add(_copy); _menu.Items.Add(_paste);
    }
    /// <inheritdoc/>
    protected override string DefaultStyleName => "Inspector.PropertyMenu";
    /// <inheritdoc/>
    protected override bool UseSkinFocus => false;
    /// <summary>Gets the menu. Applications may append type-specific commands.</summary>
    public ContextMenu Menu => _menu;
    /// <summary>Gets or sets an explicit reset override, for example removing a control's local style assignment.</summary>
    public Action? ResetAction { get => _resetAction; set => SetNotify(ref _resetAction, value, ResetActionArgs); }
    /// <summary>Gets whether a reset action is available for the current attached field.</summary>
    public bool CanReset => !_readOnly && IsCurrent && (_actions is not null || ResetAction is not null);
    /// <summary>Gets whether this field has typed copy support.</summary>
    public bool CanCopy => IsCurrent && _actions is not null;
    /// <summary>Gets whether the clipboard contains the exact property type.</summary>
    public bool CanPaste => !_readOnly && IsCurrent && Workspace is Workspace owner && (_actions?.CanPaste(owner.Registry.InspectorClipboard) ?? false);
    private bool IsCurrent => Workspace is not null && _inspector.OwnsMenu(_editor, this);
    /// <summary>Restores the configured default or invokes the explicit style-reset callback.</summary>
    public void ResetValue()
    {
        if (!CanReset) return;
        if (ResetAction is Action reset) reset(); else _actions!.Reset();
    }
    /// <summary>Copies the complete typed value to the registry-local property clipboard.</summary>
    public void CopyValue() { if (CanCopy) _actions!.Copy(Workspace!.Registry.InspectorClipboard); }
    /// <summary>Pastes a matching typed value; incompatible values leave the model unchanged.</summary>
    public void PasteValue() { if (CanPaste) _actions!.Paste(Workspace!.Registry.InspectorClipboard); }
    /// <inheritdoc/>
    protected override void OnClick()
    {
        RaiseClicked();
        if (!IsCurrent || !IsVisible || _menu.Workspace is not null) return;
        // Availability is evaluated only when opening or executing the menu.
        _reset.IsEnabled = CanReset; _copy.IsEnabled = CanCopy; _paste.IsEnabled = CanPaste;
        _menu.Show(Workspace!, new Vector2(Bounds.Right, Bounds.Bottom), this);
    }
    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        Color background = !IsAvailable() ? DisabledBackground : IsHeld ? PressedBackground : IsPointerOver ? HoverBackground : Background;
        context.FillRectangle(LocalBounds, background);
        Color color = IsAvailable() ? TextColor : DisabledTextColor;
        float x = Bounds.Width / 2 - 1.5f, y = Bounds.Height / 2 - 7.5f;
        for (int i = 0; i < 3; i++)
            context.FillRectangle(new Bounds2(x, y + i * 6, 3, 3), color);
    }
    /// <inheritdoc/>
    protected override void OnDetached() { if (_menu.Workspace is Workspace owner) owner.ClosePopup(_menu); base.OnDetached(); }
    internal void SetMenuVisible(bool visible)
    {
        if (!visible && _menu.Workspace is Workspace owner) owner.ClosePopup(_menu);
        IsVisible = visible;
        if (Parent is not Grid row) return;
        if (!visible && row.Tracks.Count == 3)
        {
            row.SetCell(this, column: 1);
            row.Tracks.RemoveAt(2);
        }
        else if (visible && row.Tracks.Count == 2)
        {
            row.Tracks.Add(new ColumnTrack { Width = new GridLength(26) });
            row.SetCell(this, column: 2);
        }
    }

    internal void SetReadOnly() => _readOnly = true;

    internal void SetDefault<T>(T value)
    {
        if (_actions is not InspectorPropertyActions<T> typed)
            throw new ArgumentException("The default value must have the exact bound property type.", nameof(value));
        typed.SetDefault(value);
    }
}
