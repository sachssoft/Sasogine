
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Sachssoft.Engine.Controls;

/// <summary>
/// Provides a file system dialog with directory navigation, file selection,
/// configurable favorite folders, and drive locations.
/// </summary>
public abstract class FileDialog : Dialog
{
    private static readonly PropertyChangedEventArgs SystemFavoritesArgs = new(nameof(UseSystemFavorites));
    private static readonly PropertyChangedEventArgs SystemDrivesArgs = new(nameof(UseSystemDrives));
    private static readonly PropertyChangedEventArgs InitialDirectoryStorageArgs = new(nameof(InitialDirectory));
    private static readonly PropertyChangedEventArgs FiltersCollectionArgs = new(nameof(Filters));
    private static readonly PropertyChangedEventArgs ProviderStorageArgs = new(nameof(Provider));

    private readonly FileDialogSidebar _sidebar = new();
    private readonly List<string> _history = new();
    private readonly TextBox _path = new();
    private readonly TextBox _name = new();
    private readonly ListBox _list = new();
    private readonly ComboBox _filter = new();
    private readonly Label _status = new();
    private readonly Button _back = new FileDialogGlyphButton("icon_left", "<");
    private readonly Button _up = new FileDialogGlyphButton("icon_up", "^");
    private readonly Button _refresh = new FileDialogGlyphButton("icon_refresh", "R");
    private readonly Button _confirm = new();
    private readonly Button _cancel = new()
    {
        Content = new TextBlock { Text = "Cancel" }
    };

    private FileDialogDirectory? _directory;
    private ElementCollection<FileDialogFilter>? _ownedFilters;
    private IFileDialogProvider? _storedProvider;
    private string _storedInitialDirectory = string.Empty;
    private int _historyIndex = -1;
    private bool _useSystemFavorites = true;
    private bool _useSystemDrives = true;
    private bool _updating;
    private bool _navigatingHistory;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileDialog"/> class.
    /// </summary>
    protected FileDialog()
    {
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

        Content = new FileDialogLayout(
            _back, _up, _refresh, _path, _list, _name,
            _filter, _status, _confirm, _cancel, _sidebar);
    }

    /// <summary>
    /// Gets the collection of custom favorite folders.
    /// System favorite folders are managed separately.
    /// </summary>
    public IList<FileDialogLocation> FavoriteFolders { get; } = new List<FileDialogLocation>();

    /// <summary>
    /// Gets the collection of custom drive locations.
    /// </summary>
    public IList<FileDialogLocation> Drives { get; } = new List<FileDialogLocation>();

    /// <summary>
    /// Gets or sets whether standard system folders are displayed
    /// alongside custom favorite folders.
    /// </summary>
    public bool UseSystemFavorites
    {
        get => _useSystemFavorites;
        set => SetNotify(ref _useSystemFavorites, value, SystemFavoritesArgs);
    }

    /// <summary>
    /// Gets or sets whether local system drives are displayed
    /// alongside custom drive locations.
    /// </summary>
    public bool UseSystemDrives
    {
        get => _useSystemDrives;
        set => SetNotify(ref _useSystemDrives, value, SystemDrivesArgs);
    }

    /// <summary>
    /// Gets the list control displaying favorite folders.
    /// </summary>
    public ListBox FavoritesList => _sidebar.Favorites;

    /// <summary>
    /// Gets the list control displaying drive locations.
    /// </summary>
    public ListBox DrivesList => _sidebar.Drives;

    /// <summary>
    /// Gets the currently selected file or directory path,
    /// or <see langword="null"/> when no selection has been accepted.
    /// </summary>
    public string? SelectedPath { get; private set; }

    /// <summary>
    /// Gets the full path of the current directory.
    /// </summary>
    public string CurrentDirectory => _directory?.FullPath ?? string.Empty;

    /// <summary>
    /// Gets the current error or status message.
    /// </summary>
    public string ErrorText => _status.Text;

    /// <summary>
    /// Gets the list control displaying directory entries.
    /// </summary>
    public ListBox FileList => _list;

    /// <summary>
    /// Gets the text box used to enter or display the selected file name.
    /// </summary>
    public TextBox FileNameBox => _name;

    /// <summary>
    /// Gets or sets the initial directory displayed when the dialog opens.
    /// </summary>
    public string InitialDirectory
    {
        get => ConfigInitialDirectory;
        set => ConfigInitialDirectory = value;
    }

    /// <summary>
    /// Gets the collection of file filters available in the dialog.
    /// </summary>
    public IList<FileDialogFilter> Filters => ConfigFilters;

    /// <summary>
    /// Gets a value indicating whether the dialog selects directories
    /// instead of files.
    /// </summary>
    protected abstract bool SelectsDirectory { get; }

    private IFileDialogProvider Provider =>
        ConfigProvider ?? Workspace?.Registry.FileSystem ?? new LocalFileDialogProvider();

    internal string ConfigInitialDirectory
    {
        get => _storedInitialDirectory;
        set => SetNotify(ref _storedInitialDirectory, value, InitialDirectoryStorageArgs);
    }

    internal IList<FileDialogFilter> ConfigFilters =>
        _ownedFilters ??= new(this, FiltersCollectionArgs);

    internal IFileDialogProvider? ConfigProvider
    {
        get => _storedProvider;
        set => SetNotify(ref _storedProvider, value, ProviderStorageArgs);
    }

    /// <inheritdoc/>
    protected override void OnOpening()
    {
        RefreshLocations();

        SelectedPath = null;
        _directory = null;
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

        string start = ConfigInitialDirectory.Length > 0
            ? ConfigInitialDirectory
            : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        if (start.Length == 0)
            start = Environment.CurrentDirectory;

        Navigate(start);
    }

    /// <summary>
    /// Navigates to the specified directory and updates the navigation history.
    /// </summary>
    /// <param name="path">The directory path to navigate to.</param>
    /// <returns>
    /// <see langword="true"/> if navigation succeeds;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Navigate(string path)
    {
        try
        {
            string resolved = _directory is null
                ? path
                : Provider.Resolve(_directory.FullPath, path);

            FileDialogDirectory snapshot = Provider.ReadDirectory(resolved);

            _directory = snapshot;
            _path.Text = snapshot.FullPath;
            _status.Text = string.Empty;
            _name.Text = string.Empty;

            if (!_navigatingHistory &&
                (_historyIndex < 0 || _history[_historyIndex] != snapshot.FullPath))
            {
                if (_historyIndex + 1 < _history.Count)
                {
                    _history.RemoveRange(
                        _historyIndex + 1,
                        _history.Count - _historyIndex - 1);
                }

                _history.Add(snapshot.FullPath);
                _historyIndex = _history.Count - 1;
            }

            RefreshItems();
            return true;
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException
            or ArgumentException or NotSupportedException)
        {
            _status.Text = exception.Message;
            return false;
        }
    }

    /// <summary>
    /// Navigates to the previous directory in the navigation history.
    /// </summary>
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

    /// <summary>
    /// Navigates to the parent directory when available.
    /// </summary>
    public void GoUp()
    {
        if (_directory?.ParentPath is string parent)
            Navigate(parent);
    }

    /// <summary>
    /// Reloads the contents of the current directory.
    /// </summary>
    public void Refresh()
    {
        if (_directory is not null)
            Navigate(_directory.FullPath);
    }

    /// <summary>
    /// Refreshes the sidebar locations using configured favorite folders,
    /// optional system folders, custom drives, and optional system drives.
    /// </summary>
    public void RefreshLocations()
    {
        _sidebar.Favorites.SelectedIndex = -1;
        _sidebar.Drives.SelectedIndex = -1;

        _sidebar.Favorites.Items.Clear();
        _sidebar.Drives.Items.Clear();

        var favoritePaths = new HashSet<string>(
            OperatingSystem.IsWindows()
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal);

        void AddFavorite(string name, string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !favoritePaths.Add(path))
                return;

            _sidebar.Favorites.Items.Add(
                new SelectorItem(
                    name,
                    new FileDialogLocation(name, path),
                    true,
                    "icon_folder"));
        }

        if (UseSystemFavorites && ConfigProvider is null)
        {
            var folders = new (Environment.SpecialFolder Folder, string Name)[]
            {
                (Environment.SpecialFolder.UserProfile, "Home"),
                (Environment.SpecialFolder.DesktopDirectory, "Desktop"),
                (Environment.SpecialFolder.MyDocuments, "Documents"),
                (Environment.SpecialFolder.MyPictures, "Pictures"),
                (Environment.SpecialFolder.MyMusic, "Music"),
                (Environment.SpecialFolder.MyVideos, "Videos")
            };

            foreach (var (folder, name) in folders)
            {
                string path = Environment.GetFolderPath(folder);

                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                    AddFavorite(name, path);
            }

            string home = Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile);

            if (!string.IsNullOrWhiteSpace(home))
            {
                string downloads = Path.Combine(home, "Downloads");

                if (Directory.Exists(downloads))
                    AddFavorite("Downloads", downloads);
            }
        }

        foreach (var location in FavoriteFolders)
            AddFavorite(location.Name, location.Path);

        foreach (var location in Drives)
        {
            _sidebar.Drives.Items.Add(
                new SelectorItem(
                    location.Name,
                    location,
                    true,
                    "icon_folder"));
        }

        if (UseSystemDrives)
        {
            try
            {
                foreach (string drive in Directory.GetLogicalDrives())
                {
                    _sidebar.Drives.Items.Add(
                        new SelectorItem(
                            drive,
                            new FileDialogLocation(drive, drive),
                            true,
                            "icon_folder"));
                }
            }
            catch (Exception exception) when (
                exception is IOException or UnauthorizedAccessException)
            {
                _status.Text = exception.Message;
            }
        }
    }

    /// <summary>
    /// Activates the selected entry, navigating into directories
    /// or accepting file selections.
    /// </summary>
    public void ActivateEntry()
    {
        if (_list.SelectedItem?.Value is not FileDialogEntry entry)
            return;

        if (entry.IsDirectory)
            Navigate(entry.FullPath);
        else if (!SelectsDirectory)
            AcceptSelection();
    }

    /// <summary>
    /// Validates and accepts the current file or directory selection.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the selection is valid and accepted;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool AcceptSelection()
    {
        if (_directory is null)
            return false;

        try
        {
            string path = _name.Text.Length == 0 && SelectsDirectory
                ? _directory.FullPath
                : Provider.Resolve(_directory.FullPath, _name.Text);

            if (!Provider.Exists(path, SelectsDirectory))
            {
                _status.Text = SelectsDirectory
                    ? "Select an existing folder."
                    : "Select an existing file.";

                return false;
            }

            if (!SelectsDirectory &&
                _filter.SelectedItem?.Value is FileDialogFilter filter &&
                !filter.Matches(Path.GetFileName(path)))
            {
                _status.Text = "The selected file does not match the active filter.";
                return false;
            }

            SelectedPath = path;
            Complete(DialogResult.OK);
            return true;
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException
            or ArgumentException or NotSupportedException)
        {
            _status.Text = exception.Message;
            return false;
        }
    }

    /// <inheritdoc/>
    protected override void OnAccept()
    {
        AcceptSelection();
    }

    internal override bool IsStyleLocal(string property)
    {
        return base.IsStyleLocal(property);
    }

    /// <summary>
    /// Removes a local style override and restores the current style value.
    /// </summary>
    /// <param name="property">The name of the style property to reset.</param>
    public override void ResetStyleProperty(string property)
    {
        base.ResetStyleProperty(property);
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
    }

    private void RefreshItems()
    {
        _list.SelectedIndex = -1;
        _list.Items.Clear();

        if (_directory is null)
            return;

        var filter = _filter.SelectedItem?.Value as FileDialogFilter;

        foreach (FileDialogEntry entry in _directory.Entries)
        {
            if (entry.IsDirectory ||
                (!SelectsDirectory && (filter is null || filter.Matches(entry.Name))))
            {
                _list.Items.Add(
                    new SelectorItem(
                        entry.Name,
                        entry,
                        true,
                        entry.IsDirectory ? "icon_folder" : "icon_file"));
            }
        }

        _up.IsEnabled = _directory.ParentPath is not null;
        _back.IsEnabled = _historyIndex > 0;
    }
}
