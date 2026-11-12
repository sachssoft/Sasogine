using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// A compact selector whose list is hosted in the root popup layer.
/// </summary>
public sealed class ComboBox : ContainerControl
{
    private ItemsProjection<SelectorItem>? _dataItems;
    private Func<object?, string>? _itemTextSelector;
    private object? _pendingSelectedValue;
    private bool _hasPendingSelectedValue;
    private SelectorItem? _notifiedSelectedItem;
    private IRecyclingItemTemplate? _itemTemplate;
    private ITemplate<ItemsControl, ContainerControl>? _itemsPanel;
    private Control? _selectedPresenter;
    private ITemplate<object?, Control>? _selectedContentTemplate;
    private ITemplate<object?, Control>? _appliedSelectedContentTemplate;
    private ITemplate<object?, string>? _selectedTextTemplate;
    private ITemplate<object?, string>? _appliedSelectedTextTemplate;
    private SelectorItem? _presentedSelection;
    private string? _selectedCaption;
    private static readonly System.ComponentModel.PropertyChangedEventArgs SelectedPresenterArgs = new("SelectedPresenter");

    private ItemsProjection<SelectorItem> DataItems => _dataItems ??= new ItemsProjection<SelectorItem>(InvalidateMeasure);

    /// <summary>Gets or sets the observable source of selectable entries.</summary>
    public System.Collections.IEnumerable? ItemsSource
    {
        get => _dataItems?.Source;
        set
        {
            VerifyTreeMutation();
            if (ReferenceEquals(_dataItems?.Source, value))
                return;
            DataItems.SetSource(value, Items.Count != 0);
            if (Workspace is not null)
                DataItems.Attach();
            OnItemPresentationChanged();
            NotifyPropertyChanged(ItemsSourceArgs);
        }
    }

    private static readonly System.ComponentModel.PropertyChangedEventArgs ItemsSourceArgs = new(nameof(ItemsSource));
    private static readonly System.ComponentModel.PropertyChangedEventArgs SelectedValueArgs = new(nameof(SelectedValue));

    /// <summary>Gets or sets the optional caption selector for data items.</summary>
    public Func<object?, string>? ItemTextSelector
    {
        get => _itemTextSelector;
        set
        {
            if (ReferenceEquals(_itemTextSelector, value))
                return;
            _itemTextSelector = value;
            DataItems.Refresh(recreate: true);
            OnItemPresentationChanged();
        }
    }

    /// <summary>Gets or sets the reusable control template for data items.</summary>
    public IRecyclingItemTemplate? ItemTemplate
    {
        get => _itemTemplate;
        set
        {
            VerifyTreeMutation();
            if (ReferenceEquals(_itemTemplate, value))
                return;
            _itemTemplate = value;
            OnItemPresentationChanged();
        }
    }

    /// <summary>Gets or sets the panel factory for templated items.</summary>
    public ITemplate<ItemsControl, ContainerControl>? ItemsPanel
    {
        get => _itemsPanel;
        set
        {
            VerifyTreeMutation();
            if (ReferenceEquals(_itemsPanel, value))
                return;
            _itemsPanel = value;
            OnItemPresentationChanged();
        }
    }

    /// <summary>Gets or sets the primary selection by its application data.</summary>
    public object? SelectedValue
    {
        get => _hasPendingSelectedValue ? _pendingSelectedValue : SelectedItem?.Value;
        set
        {
            if (_dataItems is { NeedsSynchronization: true })
            {
                _pendingSelectedValue = value;
                _hasPendingSelectedValue = true;
                return;
            }
            int index = -1;
            if (value is not null)
                for (int i = 0; i < Items.Count; i++)
                    if (ReferenceEquals(Items[i].Value, value) || Equals(Items[i].Value, value))
                    {
                        index = i;
                        break;
                    }
            SelectedIndex = index;
        }
    }

    /// <summary>Refreshes a data source without collection-change notifications.</summary>
    public void RefreshItems()
    {
        _dataItems?.Refresh();
        if (ItemsSource is not null)
            _popup?.RefreshItems();
    }

    /// <inheritdoc/>
    protected override void OnAttached()
    {
        base.OnAttached();
        _dataItems?.Attach();
    }

    /// <inheritdoc/>
    protected override void OnDetached()
    {
        _dataItems?.Detach();
        base.OnDetached();
    }

    /// <inheritdoc/>
    internal override void Prepare()
    {
        base.Prepare();
        SelectorItem? selected = SelectedItem;
        if (_dataItems?.Synchronize(ConfigItems, CreateSelectorItem) == true)
        {
            int index = selected is null ? SelectedIndex : Items.IndexOf(selected);
            if (index < 0 && selected?.Value is { } value)
                SelectedValue = value;
            else if (index != SelectedIndex)
                SelectedIndex = index;
        }
        if (_hasPendingSelectedValue && _dataItems?.NeedsSynchronization != true)
        {
            object? requested = _pendingSelectedValue;
            _pendingSelectedValue = null;
            _hasPendingSelectedValue = false;
            SelectedValue = requested;
        }
        PrepareSelectedPresentation();
    }

    /// <summary>Builds the legacy selection descriptor for one application data item.</summary>
    /// <param name="item">The application data.</param>
    /// <returns>The descriptor used for selection and plain text presentation.</returns>
    private SelectorItem CreateSelectorItem(object? item) => item is SelectorItem selector ? selector with { }
        : new SelectorItem(ItemTextSelector?.Invoke(item) ?? item?.ToString() ?? string.Empty, item);

    /// <summary>Discards a popup when its source or template configuration changes.</summary>
    private void OnItemPresentationChanged()
    {
        CloseDropDown();
        InvalidateMeasure();
    }

    /// <summary>Formats the selected data item independently from the popup row.</summary>
    public ITemplate<object?, string>? SelectedTextTemplate
    {
        get => _selectedTextTemplate;
        set
        {
            if (ReferenceEquals(_selectedTextTemplate, value))
                return;
            _selectedTextTemplate = value;
            InvalidateMeasure();
        }
    }

    /// <summary>Gets or sets the control template displayed while the dropdown is closed.</summary>
    public ITemplate<object?, Control>? SelectedContentTemplate
    {
        get => _selectedContentTemplate;
        set
        {
            VerifyTreeMutation();
            if (ReferenceEquals(_selectedContentTemplate, value))
                return;
            _selectedContentTemplate = value;
            InvalidateMeasure();
        }
    }

    /// <summary>Refreshes the selected item display during update, outside drawing.</summary>
    private void PrepareSelectedPresentation()
    {
        SelectorItem? selected = SelectedItem;
        if (ReferenceEquals(selected, _presentedSelection) &&
            ReferenceEquals(_appliedSelectedContentTemplate, SelectedContentTemplate) &&
            ReferenceEquals(_appliedSelectedTextTemplate, SelectedTextTemplate))
            return;

        object? item = selected?.Value ?? selected;
        Control? presentation = selected is not null && SelectedContentTemplate is { } content
            ? content.Create(item) ?? throw new InvalidOperationException("The selection template returned null.") : null;
        if (presentation is not null)
        {
            presentation.DataContext = item;
            presentation.IsHitTestVisible = false;
            presentation.IsFocusable = false;
        }
        _selectedCaption = selected is not null ? SelectedTextTemplate?.Create(item) : null;
        SetOwnedControl(ref _selectedPresenter, presentation, SelectedPresenterArgs);
        _presentedSelection = selected;
        _appliedSelectedContentTemplate = SelectedContentTemplate;
        _appliedSelectedTextTemplate = SelectedTextTemplate;
    }

    private ListBox? _popup;
    private IControlFont? _font;
    private int _last = -1;
    /// <summary>
    /// Initializes a new combo box instance.
    /// </summary>
    public ComboBox() : base()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "ComboBox";
    /// <summary>
    /// Gets items.
    /// </summary>
    public IList<SelectorItem> Items => _dataItems?.OwnsItems == true
        ? _readOnlyItems ??= new System.Collections.ObjectModel.ReadOnlyCollection<SelectorItem>(ConfigItems)
        : ConfigItems;

    private IList<SelectorItem>? _readOnlyItems;
    /// <summary>
    /// Gets is drop down open.
    /// </summary>
    public bool IsDropDownOpen => _popup is not null;

    /// <summary>
    /// Gets or sets selected index.
    /// </summary>
    public int SelectedIndex
    {
        get
        {
            int i = ConfigSelectedIndex;
            return i >= 0 && i < Items.Count && Items[i].IsEnabled ? i : -1;
        }

        set
        {
            if (value < -1 || value >= Items.Count)
                throw new ArgumentOutOfRangeException(nameof(value));
            if (value >= 0 && !Items[value].IsEnabled)
                return;
            ConfigSelectedIndex = value;
            Notify();
        }
    }

    /// <summary>
    /// Gets selected item.
    /// </summary>
    public SelectorItem? SelectedItem => SelectedIndex < 0 ? null : Items[SelectedIndex];

    /// <summary>
    /// Occurs when selection changed.
    /// </summary>
    public event EventHandler? SelectionChanged;
    private void Notify()
    {
        SelectorItem? selected = SelectedItem;
        if (_last == SelectedIndex && ReferenceEquals(_notifiedSelectedItem, selected))
            return;
        _last = SelectedIndex;
        _notifiedSelectedItem = selected;
        if (SelectedContentTemplate is not null || SelectedTextTemplate is not null)
            InvalidateMeasure();
        NotifyPropertyChanged(SelectedValueArgs);
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Performs open drop down using the current control state.
    /// </summary>
    public void OpenDropDown()
    {
        if (_popup is not null || Workspace is null || !IsAvailable())
            return;
        var list = new ListBox()
        {
            Height = MathF.Min(ConfigMaxDropDownHeight, MathF.Max(1, Items.Count) * ConfigItemHeight),
            Width = Bounds.Width,
            Padding = new Insets2(2)
        };
        if (ItemsSource is not null)
        {
            list.ItemsSource = ItemsSource;
            list.ItemTextSelector = ItemTextSelector;
        }
        else
            foreach (SelectorItem item in Items)
                list.Items.Add(item);
        list.ItemTemplate = ItemTemplate;
        list.ItemsPanel = ItemsPanel;
        list.SetPopupSelection(SelectedIndex);
        list.ItemActivated += (_, _) =>
        {
            if (list.SelectedIndex >= 0)
                SelectedIndex = list.SelectedIndex;
            CloseDropDown();
        };
        _popup = list;
        Workspace.ShowPopup(list, new Bounds2(Bounds.X, Bounds.Bottom, Bounds.Width, list.Height ?? 0), this, () => _popup = null);
    }

    /// <summary>
    /// Performs close drop down using the current control state.
    /// </summary>
    public void CloseDropDown()
    {
        if (_popup is ListBox popup)
            Workspace?.ClosePopup(popup);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        _font = GetFont(context);
        _selectedPresenter?.Measure(context, new Size2(MathF.Max(0, available.Width - 28), available.Height));
        return new Size2(MathF.Max(160, (_selectedPresenter?.DesiredSize.Width ?? 0) + 28),
            MathF.Max(MathF.Max(20, _font.Measure("Mg").Height), _selectedPresenter?.DesiredSize.Height ?? 0));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds) =>
        _selectedPresenter?.Arrange(new Bounds2(bounds.X, bounds.Y, MathF.Max(0, bounds.Width - 28), bounds.Height), true);

    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        Notify();
        if (!IsAvailable())
            CloseDropDown();
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var e = args.Input;
        if (e.Type == ControlInputEventType.PointerDown && e.Button == MouseButton.Left)
        {
            if (IsDropDownOpen)
                CloseDropDown();
            else
                OpenDropDown();
            args.Handled = true;
            return;
        }

        if (e.Type != ControlInputEventType.KeyDown)
            return;
        if (e.Key is Keys.F4 or Keys.Space or Keys.Enter || e.Alt && e.Key == Keys.Down)
        {
            OpenDropDown();
            args.Handled = true;
            return;
        }

        if (e.Key is Keys.Up or Keys.Down)
        {
            int direction = e.Key == Keys.Down ? 1 : -1, index = SelectedIndex;
            do
            {
                index += direction;
            }
            while (index >= 0 && index < Items.Count && !Items[index].IsEnabled);
            if (index >= 0 && index < Items.Count)
                SelectedIndex = index;
            args.Handled = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        string state = !IsAvailable() ? "Disabled" : IsFocused ? "Focused" : IsPointerOver ? "Hover" : "Normal";
        if (!context.DrawSkinPart(this, "Background", LocalBounds, state))
        {
            context.FillRectangle(LocalBounds, Background);
        }

        context.DrawSkinPart(this, "DropDownGlyph", new Bounds2(Bounds.Width - 27, (Bounds.Height - 20) / 2, 20, 20));
        var font = GetFont(context);
        if (_selectedPresenter is not null)
            return;
        string text = _selectedCaption ?? SelectedItem?.Text ?? ConfigPlaceholder;
        using (context.PushClipScope(new Bounds2(Padding.Left, Padding.Top, MathF.Max(0, Bounds.Width - Padding.Horizontal - 28), MathF.Max(0, Bounds.Height - Padding.Vertical))))
            context.DrawText(font, text, new Vector2(Padding.Left, (Bounds.Height - font.Measure(text).Height) / 2), IsAvailable() ? ConfigTextColor : ConfigDisabledTextColor);
    }

    private StyleProperty<string> _stylePlaceholder = new(string.Empty);
    private static readonly System.ComponentModel.PropertyChangedEventArgs PlaceholderChangedArgs = new(nameof(Placeholder));
    internal string ConfigPlaceholder
    {
        get => _stylePlaceholder.Value;
        set
        {
            if (_stylePlaceholder.SetLocal(value))
                NotifyPropertyChanged(PlaceholderChangedArgs);
        }
    }

    /// <summary>
    /// Gets or sets Placeholder; a local assignment overrides the style.
    /// </summary>
    public string Placeholder { get => ConfigPlaceholder; set => ConfigPlaceholder = value; }

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

    private StyleProperty<int> _styleSelectedIndex = new(-1);
    private static readonly System.ComponentModel.PropertyChangedEventArgs SelectedIndexChangedArgs = new(nameof(SelectedIndex));
    internal int ConfigSelectedIndex
    {
        get => _styleSelectedIndex.Value;
        set
        {
            if (_styleSelectedIndex.SetLocal(value))
                NotifyPropertyChanged(SelectedIndexChangedArgs);
        }
    }

    private StyleProperty<float> _styleMaxDropDownHeight = new(240f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs MaxDropDownHeightChangedArgs = new(nameof(MaxDropDownHeight));
    internal float ConfigMaxDropDownHeight
    {
        get => _styleMaxDropDownHeight.Value;
        set
        {
            if (_styleMaxDropDownHeight.SetLocal(value))
                NotifyPropertyChanged(MaxDropDownHeightChangedArgs);
        }
    }

    /// <summary>
    /// Gets or sets MaxDropDownHeight; a local assignment overrides the style.
    /// </summary>
    public  float MaxDropDownHeight
    {
        get => ConfigMaxDropDownHeight;
        set => ConfigMaxDropDownHeight = value;
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
                case "Placeholder":
                    if (_stylePlaceholder.Apply(Converters.Convert<string>("String", entry.Value)))
                        NotifyPropertyChanged(PlaceholderChangedArgs);
                    break;
                case "ItemHeight":
                    if (_styleItemHeight.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(ItemHeightChangedArgs);
                    break;
                case "SelectedIndex":
                    if (_styleSelectedIndex.Apply(Converters.Convert<int>("Int32", entry.Value)))
                        NotifyPropertyChanged(SelectedIndexChangedArgs);
                    break;
                case "MaxDropDownHeight":
                    if (_styleMaxDropDownHeight.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(MaxDropDownHeightChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "Placeholder":
                return _stylePlaceholder.HasLocalValue;
            case "ItemHeight":
                return _styleItemHeight.HasLocalValue;
            case "SelectedIndex":
                return _styleSelectedIndex.HasLocalValue;
            case "MaxDropDownHeight":
                return _styleMaxDropDownHeight.HasLocalValue;
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
            case "Placeholder":
                if (_stylePlaceholder.ClearLocal())
                    NotifyPropertyChanged(PlaceholderChangedArgs);
                return;
            case "ItemHeight":
                if (_styleItemHeight.ClearLocal())
                    NotifyPropertyChanged(ItemHeightChangedArgs);
                return;
            case "SelectedIndex":
                if (_styleSelectedIndex.ClearLocal())
                    NotifyPropertyChanged(SelectedIndexChangedArgs);
                return;
            case "MaxDropDownHeight":
                if (_styleMaxDropDownHeight.ClearLocal())
                    NotifyPropertyChanged(MaxDropDownHeightChangedArgs);
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
        if (_stylePlaceholder.BeginStyle(string.Empty))
            NotifyPropertyChanged(PlaceholderChangedArgs);
        if (_styleItemHeight.BeginStyle(30f))
            NotifyPropertyChanged(ItemHeightChangedArgs);
        if (_styleSelectedIndex.BeginStyle(-1))
            NotifyPropertyChanged(SelectedIndexChangedArgs);
        if (_styleMaxDropDownHeight.BeginStyle(240f))
            NotifyPropertyChanged(MaxDropDownHeightChangedArgs);
    }

    private ElementCollection<SelectorItem>? _ownedItems;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ItemsCollectionArgs = new("Items");
    internal IList<SelectorItem> ConfigItems => _ownedItems ??= new(this, ItemsCollectionArgs);

    /// <summary>
    /// Commits final style values and notifies once per effective property change.
    /// </summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_stylePlaceholder.CommitStyle())
            NotifyPropertyChanged(PlaceholderChangedArgs);
        if (_styleItemHeight.CommitStyle())
            NotifyPropertyChanged(ItemHeightChangedArgs);
        if (_styleSelectedIndex.CommitStyle())
            NotifyPropertyChanged(SelectedIndexChangedArgs);
        if (_styleMaxDropDownHeight.CommitStyle())
            NotifyPropertyChanged(MaxDropDownHeightChangedArgs);
    }
}
