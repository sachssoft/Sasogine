using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sachssoft.Engine.Controls;
/// <summary>Custom-skinned directory navigation with validated results and explicit filesystem snapshots.</summary>
public abstract class FileDialog : Dialog
{
    private readonly FileDialogSidebar _sidebar = new();
    /// <summary>Gets custom favorite locations. Defaults are added once when the dialog is created.</summary>
    public IList<FileDialogLocation> Favorites { get; } = new List<FileDialogLocation>();
    /// <summary>Gets custom drive locations. Set UseSystemDrives to false for virtual providers.</summary>
    public IList<FileDialogLocation> Drives { get; } = new List<FileDialogLocation>();
    /// <summary>Gets or sets whether local system drives are shown along with custom drives.</summary>
    public bool UseSystemDrives { get => _useSystemDrives; set => SetNotify(ref _useSystemDrives, value, SystemDrivesArgs); }

    private bool _useSystemDrives = true;
    private static readonly System.ComponentModel.PropertyChangedEventArgs SystemDrivesArgs = new(nameof(UseSystemDrives));
    /// <summary>Gets the favorites list for selection and deterministic polling.</summary>
    public ListBox FavoritesList => _sidebar.Favorites;
    /// <summary>Gets the drive list for selection and deterministic polling.</summary>
    public ListBox DrivesList => _sidebar.Drives;

    private FileDialogDirectory? _directory;
    private readonly List<string> _history = new();
    private int _historyIndex = -1;
    private bool _updating, _navigatingHistory;
    private readonly TextBox _path = new(), _name = new();
    private readonly ListBox _list = new();
    private readonly ComboBox _filter = new();
    private readonly Label _status = new();
    private readonly Button _back = new FileDialogGlyphButton("icon_left", "<"), _up = new FileDialogGlyphButton("icon_up", "^"), _refresh = new FileDialogGlyphButton("icon_refresh", "R");
    private readonly Button _confirm = new(), _cancel = new()
    {
        Content = new TextBlock
        {
            Text = "Cancel"
        }
    };
    /// <summary>Initializes a new file dialog instance.</summary>
    protected FileDialog() : base()
    {
        foreach (var folder in new[]
        {
            Environment.SpecialFolder.UserProfile,
            Environment.SpecialFolder.DesktopDirectory,
            Environment.SpecialFolder.MyDocuments
        }

        )
        {
            string path = Environment.GetFolderPath(folder);
            if (path.Length > 0 && !Favorites.Any(x => x.Path == path))
                Favorites.Add(new FileDialogLocation(folder == Environment.SpecialFolder.UserProfile ? "Home" : folder == Environment.SpecialFolder.DesktopDirectory ? "Desktop" : "Documents", path));
        }

        _sidebar.Favorites.SelectionChanged += (_, _) =>
        {
            if (_sidebar.Favorites.SelectedItem?.Value is FileDialogLocation location)
                Navigate(location.Path);
        };
        _sidebar.Drives.SelectionChanged += (_, _) =>
        {
            if (_sidebar.Drives.SelectedItem?.Value is FileDialogLocation location)
                Navigate(location.Path);
        };
        _sidebar.Favorites.ItemActivated += (_, _) =>
        {
            if (_sidebar.Favorites.SelectedItem?.Value is FileDialogLocation location)
                Navigate(location.Path);
        };
        _sidebar.Drives.ItemActivated += (_, _) =>
        {
            if (_sidebar.Drives.SelectedItem?.Value is FileDialogLocation location)
                Navigate(location.Path);
        };
        _confirm.Content = new TextBlock
        {
            Text = SelectsDirectory ? "Select folder" : "Open"
        };
        _back.Clicked += (_, _) => GoBack();
        _up.Clicked += (_, _) => GoUp();
        _refresh.Clicked += (_, _) => Refresh();
        _confirm.Clicked += (_, _) => AcceptSelection();
        _cancel.Clicked += (_, _) => Close();
        DefaultButton = _confirm;
        _path.Submitted += (_, _) => Navigate(_path.Text);
        _name.Submitted += (_, _) => AcceptSelection();
        _list.SelectionChanged += (_, _) =>
        {
            if (_list.SelectedItem?.Value is FileDialogEntry entry)
                _name.Text = entry.Name;
        };
        _list.ItemDoubleClicked += (_, _) => ActivateEntry();
        _list.ItemActivated += (_, _) =>
        {
            if (_list.IsKeyboardActivation)
                ActivateEntry();
        };
        _filter.SelectionChanged += (_, _) =>
        {
            if (!_updating)
                RefreshItems();
        };
        Content = new FileDialogLayout(_back, _up, _refresh, _path, _list, _name, _filter, _status, _confirm, _cancel, _sidebar);
    }

    /// <summary>Gets selects directory.</summary>
    protected abstract bool SelectsDirectory { get; }
    private IFileDialogProvider Provider => ConfigProvider ?? Workspace?.Registry.FileSystem ?? new LocalFileDialogProvider();
    /// <summary>Gets or sets selected path.</summary>
    public string? SelectedPath { get; private set; }
    /// <summary>Gets current directory.</summary>
    public string CurrentDirectory => _directory?.FullPath ?? string.Empty;
    /// <summary>Gets error text.</summary>
    public string ErrorText => _status.Text;
    /// <summary>Gets file list.</summary>
    public ListBox FileList => _list;
    /// <summary>Gets file name box.</summary>
    public TextBox FileNameBox => _name;

    /// <inheritdoc/>
    protected override void OnOpening()
    {
        RefreshLocations();
        SelectedPath = null;
        _history.Clear();
        _historyIndex = -1;
        _name.Text = string.Empty;
        _updating = true;
        _filter.Items.Clear();
        foreach (var filter in ConfigFilters)
            _filter.Items.Add(new SelectorItem(filter.Name, filter));
        if (_filter.Items.Count == 0)
            _filter.Items.Add(new SelectorItem(SelectsDirectory ? "Folders" : "All files"));
        _filter.SelectedIndex = 0;
        _updating = false;
        string start = ConfigInitialDirectory.Length > 0 ? ConfigInitialDirectory : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (start.Length == 0)
            start = Environment.CurrentDirectory;
        Navigate(start);
    }

    /// <summary>Performs navigate using the current control state.</summary>
    public bool Navigate(string path)
    {
        try
        {
            string resolved = _directory is null ? path : Provider.Resolve(_directory.FullPath, path);
            FileDialogDirectory snapshot = Provider.ReadDirectory(resolved);
            _directory = snapshot;
            _path.Text = snapshot.FullPath;
            _status.Text = string.Empty;
            _name.Text = string.Empty;
            if (!_navigatingHistory && (_historyIndex < 0 || _history[_historyIndex] != snapshot.FullPath))
            {
                if (_historyIndex + 1 < _history.Count)
                    _history.RemoveRange(_historyIndex + 1, _history.Count - _historyIndex - 1);
                _history.Add(snapshot.FullPath);
                _historyIndex = _history.Count - 1;
            }

            RefreshItems();
            return true;
        }
        catch (Exception exception)when (exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            _status.Text = exception.Message;
            return false;
        }
    }

    /// <summary>Performs go back using the current control state.</summary>
    public void GoBack()
    {
        if (_historyIndex <= 0)
            return;
        int previous = _historyIndex;
        _historyIndex--;
        _navigatingHistory = true;
        try
        {
            if (!Navigate(_history[_historyIndex]))
                _historyIndex = previous;
        }
        finally
        {
            _navigatingHistory = false;
        }
    }

    /// <summary>Performs go up using the current control state.</summary>
    public void GoUp()
    {
        if (_directory?.ParentPath is string parent)
            Navigate(parent);
    }

    /// <summary>Performs refresh using the current control state.</summary>
    public void Refresh()
    {
        if (_directory is not null)
            Navigate(_directory.FullPath);
    }

    /// <summary>Refreshes navigation shortcuts after editing Favorites or Drives.</summary>
    public void RefreshLocations()
    {
        _sidebar.Favorites.SelectedIndex = -1;
        _sidebar.Drives.SelectedIndex = -1;
        _sidebar.Favorites.Items.Clear();
        _sidebar.Drives.Items.Clear();
        foreach (var location in Favorites)
            _sidebar.Favorites.Items.Add(new SelectorItem(location.Name, location, true, "icon_folder"));
        foreach (var location in Drives)
            _sidebar.Drives.Items.Add(new SelectorItem(location.Name, location, true, "icon_folder"));
        if (UseSystemDrives)
        {
            try
            {
                foreach (string drive in Directory.GetLogicalDrives())
                    _sidebar.Drives.Items.Add(new SelectorItem(drive, new FileDialogLocation(drive, drive), true, "icon_folder"));
            }
            catch (Exception exception)when (exception is IOException or UnauthorizedAccessException)
            {
                _status.Text = exception.Message;
            }
        }
    }

    private void RefreshItems()
    {
        _list.SelectedIndex = -1;
        _list.Items.Clear();
        if (_directory is null)
            return;
        var filter = _filter.SelectedItem?.Value as FileDialogFilter;
        foreach (FileDialogEntry entry in _directory.Entries)
            if (entry.IsDirectory || !SelectsDirectory && (filter is null || filter.Matches(entry.Name)))
                _list.Items.Add(new SelectorItem(entry.Name, entry, true, entry.IsDirectory ? "icon_folder" : "icon_file"));
        _up.IsEnabled = _directory.ParentPath is not null;
        _back.IsEnabled = _historyIndex > 0;
    }

    /// <summary>Performs activate entry using the current control state.</summary>
    public void ActivateEntry()
    {
        if (_list.SelectedItem?.Value is not FileDialogEntry entry)
            return;
        if (entry.IsDirectory)
            Navigate(entry.FullPath);
        else if (!SelectsDirectory)
            AcceptSelection();
    }

    /// <summary>Performs accept selection using the current control state.</summary>
    public bool AcceptSelection()
    {
        if (_directory is null)
            return false;
        try
        {
            string path = _name.Text.Length == 0 && SelectsDirectory ? _directory.FullPath : Provider.Resolve(_directory.FullPath, _name.Text);
            if (!Provider.Exists(path, SelectsDirectory))
            {
                _status.Text = SelectsDirectory ? "Select an existing folder." : "Select an existing file.";
                return false;
            }

            if (!SelectsDirectory && _filter.SelectedItem?.Value is FileDialogFilter filter && !filter.Matches(Path.GetFileName(path)))
            {
                _status.Text = "The selected file does not match the active filter.";
                return false;
            }

            SelectedPath = path;
            Complete(DialogResult.OK);
            return true;
        }
        catch (Exception exception)when (exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            _status.Text = exception.Message;
            return false;
        }
    }

    /// <inheritdoc/>
    protected override void OnAccept() => AcceptSelection();
    private string _storedInitialDirectory = string.Empty;
    private static readonly System.ComponentModel.PropertyChangedEventArgs InitialDirectoryStorageArgs = new("InitialDirectory");
    internal string ConfigInitialDirectory { get => _storedInitialDirectory; set => SetNotify(ref _storedInitialDirectory, value, InitialDirectoryStorageArgs); }
    /// <summary>Gets or sets the value for InitialDirectory.</summary>
    public string InitialDirectory { get => ConfigInitialDirectory; set => ConfigInitialDirectory = value; }

    private ElementCollection<FileDialogFilter>? _ownedFilters;
    private static readonly System.ComponentModel.PropertyChangedEventArgs FiltersCollectionArgs = new("Filters");
    internal IList<FileDialogFilter> ConfigFilters => _ownedFilters ??= new(this, FiltersCollectionArgs);
    /// <summary>Gets the owned collection for Filters.</summary>
    public IList<FileDialogFilter> Filters { get => ConfigFilters; }

    private IFileDialogProvider? _storedProvider = null;
    private static readonly System.ComponentModel.PropertyChangedEventArgs ProviderStorageArgs = new("Provider");
    internal IFileDialogProvider? ConfigProvider { get => _storedProvider; set => SetNotify(ref _storedProvider, value, ProviderStorageArgs); }

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
