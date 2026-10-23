using System.ComponentModel;

namespace Sachssoft.Engine.Controls;

/// <summary>Displays a content group with a customizable, non-collapsible header.</summary>
public class Header : GroupBox
{
    private readonly ContentControl _header = new();

    /// <summary>Initializes a header group with permanently visible content.</summary>
    public Header()
    {
        base.Header = _header;
    }

    /// <summary>Gets or sets the content displayed inside the header.</summary>
    public object? HeaderContent
    {
        get => _header.Content;
        set
        {
            if (ReferenceEquals(_header.Content, value))
                return;

            _header.Content = value;
            NotifyPropertyChanged(HeaderContentArgs);
        }
    }

    /// <summary>Gets the control responsible for presenting the header content.</summary>
    public Control? HeaderContentPresenter => _header.ContentPresenter;

    private static readonly PropertyChangedEventArgs HeaderContentArgs = new(nameof(HeaderContent));

    /// <inheritdoc/>
    protected override string DefaultStyleName => "Header";
}