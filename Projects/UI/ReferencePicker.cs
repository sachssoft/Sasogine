using System.ComponentModel;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// Displays an identifier with browse/clear controls and a filtered asset picker.
/// </summary>
public class ReferencePicker : ContentControl
{
    private IReadOnlyList<AssetItem> _source = Array.Empty<AssetItem>();
    private Predicate<AssetItem>? _filter;
    private string? _referenceId;
    private bool _allowClear = true;
    private bool _resolved;
    private static readonly PropertyChangedEventArgs IsResolvedArgs = new(nameof(IsResolved));
    private static readonly PropertyChangedEventArgs SourceArgs = new(nameof(Source)), FilterArgs = new(nameof(Filter)), ReferenceIdArgs = new(nameof(ReferenceId)), AllowClearArgs = new(nameof(AllowClear));
    /// <summary>
    /// Creates the compact reference field.
    /// </summary>
    public ReferencePicker()
    {
        var grid = new Grid
        {
            Spacing = 4
        };
        grid.Columns.Add(new GridColumn { Width = GridLength.Proportion() });
        grid.Columns.Add(new GridColumn { Width = new GridLength(74) });
        grid.Columns.Add(new GridColumn { Width = new GridLength(32) });
        grid.Children.Add(Identifier);
        grid.Cells.Put(Identifier, column: 0);
        grid.Children.Add(BrowseButton);
        grid.Cells.Put(BrowseButton, column: 1);
        grid.Children.Add(ClearButton);
        grid.Cells.Put(ClearButton, column: 2);
        BrowseButton.Clicked += (_, _) => Browse();
        ClearButton.Clicked += (_, _) => ClearReference();
        Content = grid;
        RefreshDisplay();
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "ReferencePicker";

    /// <summary>
    /// Gets or sets the available metadata snapshot.
    /// </summary>
    public IReadOnlyList<AssetItem> Source
    {
        get => _source;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (SetNotify(ref _source, value, SourceArgs))
                RefreshDisplay();
        }
    }

    /// <summary>
    /// Gets or sets an additional application filter.
    /// </summary>
    public Predicate<AssetItem>? Filter
    {
        get => _filter;
        set
        {
            if (SetNotify(ref _filter, value, FilterArgs))
                RefreshDisplay();
        }
    }

    /// <summary>
    /// Gets or sets the referenced identifier, including unresolved identifiers.
    /// </summary>
    public string? ReferenceId
    {
        get => _referenceId;
        set
        {
            if (SetNotify(ref _referenceId, string.IsNullOrEmpty(value) ? null : value, ReferenceIdArgs))
            {
                RefreshDisplay();
                OnReferenceChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the user may clear this optional reference.
    /// </summary>
    public bool AllowClear
    {
        get => _allowClear;
        set
        {
            if (SetNotify(ref _allowClear, value, AllowClearArgs))
                RefreshDisplay();
        }
    }

    /// <summary>
    /// Gets whether the current identifier resolves to a compatible source entry.
    /// </summary>
    public bool IsResolved => _resolved;
    /// <summary>
    /// Gets the read-only display field.
    /// </summary>
    public TextBox Identifier { get; } = new()
    {
        IsReadOnly = true,
        Height = 32
    };
    /// <summary>
    /// Gets the browse button.
    /// </summary>
    public Button BrowseButton { get; } = new()
    {
        Content = new TextBlock
        {
            Text = "Browse"
        }
    };
    /// <summary>
    /// Gets the clear button.
    /// </summary>
    public Button ClearButton { get; } = new()
    {
        Content = new TextBlock
        {
            Text = "×"
        }
    };
    /// <summary>
    /// Gets the currently open asset dialog.
    /// </summary>
    public AssetPickerDialog? ActiveDialog { get; private set; }

    /// <summary>
    /// Occurs after the referenced identifier changes.
    /// </summary>
    public event EventHandler? ReferenceChanged;
    /// <summary>
    /// Checks compatibility through explicit typed overrides, without reflection.
    /// </summary>
    protected virtual bool Accepts(AssetItem asset) => true;
    /// <summary>
    /// Checks whether a drag payload or explicit selection is compatible.
    /// </summary>
    public bool CanSelectAsset(AssetItem asset)
    {
        ArgumentNullException.ThrowIfNull(asset);
        return Matches(asset);
    }

    private bool Matches(AssetItem asset) => Accepts(asset) && (Filter is null || Filter(asset));
    private AssetItem? FindReference()
    {
        foreach (AssetItem asset in Source)
            if (asset.Id == ReferenceId && Matches(asset))
                return asset;
        return null;
    }

    /// <summary>
    /// Refreshes names and availability after changing an existing source collection.
    /// </summary>
    public void RefreshDisplay()
    {
        AssetItem? asset = FindReference();
        SetNotify(ref _resolved, asset is not null, IsResolvedArgs);
        Identifier.Text = ReferenceId is null ? "None" : asset?.Name ?? "Missing: " + ReferenceId;
        Identifier.ToolTip = ReferenceId ?? "No reference";
        ClearButton.IsEnabled = AllowClear && ReferenceId is not null;
    }

    /// <summary>
    /// Selects a compatible asset. No asset resource is loaded.
    /// </summary>
    public void SelectAsset(AssetItem asset)
    {
        ArgumentNullException.ThrowIfNull(asset);
        if (!Matches(asset))
            throw new ArgumentException("The asset is incompatible with this reference.", nameof(asset));
        ReferenceId = asset.Id;
    }

    /// <summary>
    /// Clears an optional reference.
    /// </summary>
    public void ClearReference()
    {
        if (AllowClear)
            ReferenceId = null;
    }

    /// <summary>
    /// Opens the shared asset picker in the owning workspace.
    /// </summary>
    public void Browse()
    {
        Workspace? owner = Workspace;
        if (owner is null || ActiveDialog is not null)
            return;
        var dialog = new AssetPickerDialog();
        dialog.Assets.Filter = Matches;
        dialog.Assets.Source = Source;
        for (int i = 0; i < dialog.Assets.Items.Count; i++)
            if (((AssetItem)dialog.Assets.Items[i].Value!).Id == ReferenceId)
            {
                dialog.Assets.SelectedIndex = i;
                break;
            }

        ActiveDialog = dialog;
        dialog.Closed += (_, _) =>
        {
            ActiveDialog = null;
            if (dialog.Result == DialogResult.OK && Workspace == owner && dialog.SelectedAsset is AssetItem asset)
                SelectAsset(asset);
        };
        _ = owner.ShowDialog(dialog);
    }

    /// <summary>
    /// Raises the reference change notification.
    /// </summary>
    protected virtual void OnReferenceChanged() => ReferenceChanged?.Invoke(this, EventArgs.Empty);
    /// <inheritdoc/>
    protected override void OnDetached()
    {
        ActiveDialog?.Close();
        base.OnDetached();
    }
}
