namespace Sachssoft.Engine.Controls;
/// <summary>Selects one compatible registered asset using the reusable preview list.</summary>
public sealed class AssetPickerDialog : Dialog
{
    /// <summary>Creates the search, list and explicit accept/cancel controls.</summary>
    public AssetPickerDialog()
    {
        var layout = new Grid { Spacing = 8 };
        layout.Rows.Add(new RowTrack { Height = new GridLength(34) });
        layout.Rows.Add(new RowTrack { Height = GridLength.Star() });
        layout.Rows.Add(new RowTrack { Height = new GridLength(34) });
        Search.TextChanged += (_, _) => Assets.FilterText = Search.Text;
        Assets.SelectionChanged += (_, _) => AcceptButton.IsEnabled = Assets.SelectedAsset is not null;
        Assets.ItemDoubleClicked += (_, _) => OnAccept();
        Assets.ItemActivated += (_, _) => { if (Assets.IsKeyboardActivation) OnAccept(); };
        AcceptButton.Clicked += (_, _) => OnAccept();
        var cancel = new Button { Content = new TextBlock { Text = "Cancel" }, Width = 100 };
        cancel.Clicked += (_, _) => Close();
        var buttons = new StackPanel { Orientation = StackOrientation.Horizontal, Spacing = 8 };
        buttons.Add(AcceptButton); buttons.Add(cancel);
        layout.Add(Search, 0); layout.Add(Assets, 1); layout.Add(buttons, 2);
        Content = layout;
        DefaultButton = AcceptButton;
    }
    /// <inheritdoc/>
    protected override string DefaultStyleName => "AssetPickerDialog";
    /// <summary>Gets the shared asset view. Supply its Source and optional Filter before showing.</summary>
    public AssetView Assets { get; } = new();
    /// <summary>Gets the name, identifier and kind search box.</summary>
    public TextBox Search { get; } = new();
    /// <summary>Gets the accept button.</summary>
    public Button AcceptButton { get; } = new() { Content = new TextBlock { Text = "Select" }, Width = 100, IsEnabled = false };
    /// <summary>Gets the accepted selection, or null before acceptance.</summary>
    public AssetItem? SelectedAsset { get; private set; }
    /// <inheritdoc/>
    protected override void OnOpening() { base.OnOpening(); SelectedAsset = null; }
    /// <inheritdoc/>
    protected override void OnAccept()
    {
        if (Assets.SelectedAsset is not AssetItem asset) return;
        SelectedAsset = asset;
        Complete(DialogResult.OK);
    }
}
