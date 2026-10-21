namespace Sachssoft.Engine.Controls;
/// <summary>A retained tab page. Unselected content is detached from frame traversal.</summary>
public class TabItem : ContentControl
{
    internal TabControl? TabOwner { get; set; }

    /// <summary>Creates a tab page.</summary>
    public TabItem()
    {
        HeaderButton.Content = HeaderCaption;
        HeaderButton.Clicked += (_, _) =>
        {
            if (TabOwner is TabControl owner)
                owner.SelectedIndex = owner.Items.IndexOf(this);
        };
    }

    internal ToggleButton HeaderButton { get; } = new()
    {
        StyleId = "TabControl.Header"
    };
    internal TextBlock HeaderCaption { get; } = new();
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
    internal Control? ConfigHeaderContent
    {
        get => _storedHeaderContent;
        set
        {
            if (ReferenceEquals(_storedHeaderContent, value))
                return;
            for (Control? ancestor = this; ancestor is not null; ancestor = ancestor.Parent)
                if (ReferenceEquals(ancestor, value))
                    throw new System.InvalidOperationException("Tab header content cannot contain its page or an ancestor.");
            // A stable header presenter owns the custom content even before a tab is shown.
            HeaderButton.Content = value ?? HeaderCaption;
            _storedHeaderContent = value;
            NotifyPropertyChanged(HeaderContentStorageArgs);
        }
    }

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
