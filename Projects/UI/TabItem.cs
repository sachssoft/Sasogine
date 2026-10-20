namespace Sachssoft.Engine.Controls;
/// <summary>A retained tab page. Unselected content is detached from frame traversal.</summary>
public class TabItem : ContentControl
{
    internal TabControl? TabOwner { get; set; }

    /// <summary>Creates a tab page.</summary>
    public TabItem() : base()
    {
    }

    /// <summary>Gets or sets header text.</summary>
    public string Header { get => ConfigHeader; set => ConfigHeader = value; }
    /// <summary>Gets whether this page is currently selected.</summary>
    public bool IsSelected => TabOwner?.SelectedItem == this;
    /// <inheritdoc/>
    protected override string DefaultStyleName => "TabItem";

    private string _storedHeader = string.Empty;
    private static readonly System.ComponentModel.PropertyChangedEventArgs HeaderStorageArgs = new("Header");
    internal string ConfigHeader { get => _storedHeader; set => SetNotify(ref _storedHeader, value, HeaderStorageArgs); }

    private Control? _storedHeaderContent = null;
    private static readonly System.ComponentModel.PropertyChangedEventArgs HeaderContentStorageArgs = new("HeaderContent");
    internal Control? ConfigHeaderContent { get => _storedHeaderContent; set => SetNotify(ref _storedHeaderContent, value, HeaderContentStorageArgs); }
    /// <summary>Gets or sets the value for HeaderContent.</summary>
    public Control? HeaderContent { get => ConfigHeaderContent; set => ConfigHeaderContent = value; }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            default:
                base.ResetStyleProperty(property);
                return;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
    }
}
