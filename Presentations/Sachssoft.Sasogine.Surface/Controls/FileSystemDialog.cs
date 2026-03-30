using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Features;
using Sachssoft.Sasogine.Interactions;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Behaviors;
using Sachssoft.Sasogine.Surface.Controls;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Utility;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using Sachssoft.Sasogine.Surface.Visuals.Controls;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using Sachssoft.Sasogine.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class FileSystemDialog : Dialog
    {
        private StyleProperty<string?> _confirmButtonText = new StyleProperty<string?>("OK", isUserSet: false);
        private StyleProperty<string?> _cancelButtonText = new StyleProperty<string?>("Cancel", isUserSet: false);
        private StyleProperty<ITextureRegion?> _folderIcon = new StyleProperty<ITextureRegion?>(null, isUserSet: false);
        private StyleProperty<ITextureRegion?> _driveIcon = new StyleProperty<ITextureRegion?>(null, isUserSet: false);
        private StyleProperty<ITextureRegion?> _fileIcon = new StyleProperty<ITextureRegion?>(null, isUserSet: false);
        private StyleProperty<string> _selectedPath = new StyleProperty<string>(Environment.CurrentDirectory, isUserSet: false);
        private StyleProperty<FileSystemDialogMode> _dialogMode = new StyleProperty<FileSystemDialogMode>(FileSystemDialogMode.OpenFile, isUserSet: false);
        private StyleProperty<IEnumerable<FileSystemDialogFilter>?> _filter = new StyleProperty<IEnumerable<FileSystemDialogFilter>?>(null, isUserSet: false);
        private StyleProperty<int> _filterIndex = new StyleProperty<int>(0, isUserSet: false);


        private readonly List<string> _history = new List<string>();

        private readonly Button _backButton = new Button();
        private readonly Button _forwardButton = new Button();
        private readonly Button _parentButton = new Button();
        private readonly Button _refreshButton = new Button();
        private readonly Image _backIcon = new Image();
        private readonly Image _forwardIcon = new Image();
        private readonly Image _parentIcon = new Image();
        private readonly Image _refreshIcon = new Image();
        private readonly ScrollViewer _expanderScrollViewer = new ScrollViewer();
        private readonly VerticalStackPanel _fsList = new VerticalStackPanel();
        private readonly VerticalStackPanel _driveList = new VerticalStackPanel();
        private readonly VerticalStackPanel _favouritePathList = new VerticalStackPanel();
        private readonly ListView _specialPathList = new ListView();
        private readonly TextBox _pathField = new TextBox();
        private readonly Label _nameLabel = new Label();
        private readonly TextBox _nameField = new TextBox();
        private readonly HorizontalStackPanel _fileFilterPanel = new HorizontalStackPanel();
        private readonly Label _filterLabel = new Label();
        private readonly ComboBox _filterField = new ComboBox();
        private readonly Grid _contentLayout = new Grid();

        private const int ImageTextSpacing = 4;
        private int _historyPosition;
        private string[]? _specialPaths =
        {
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
            Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)
        };
        private string[]? _favouritePaths = [];
        private bool _confirmed;

        private record PathInfo(string Path, bool IsDrive);

        public FileSystemDialog() : base()
        {
            Buttons.Add(new DialogButtonItem(_confirmButtonText.Value ?? "OK", 1));
            Buttons.Add(new DialogButtonItem(_cancelButtonText.Value ?? "Cancel", 0));

            Title = "File Choose Dialog";

            BuildUI();
        }

        #region Style Properties

        public StyleProperty<string?> ConfirmButtonText
        {
            get => _confirmButtonText;
            set => SetAndNotify(ref _confirmButtonText, value);
        }

        public StyleProperty<string?> CancelButtonText
        {
            get => _cancelButtonText;
            set => SetAndNotify(ref _cancelButtonText, value);
        }

        public StyleProperty<ITextureRegion?> FolderIcon
        {
            get => _folderIcon;
            set => SetAndNotify(ref _folderIcon, value);
        }

        public StyleProperty<ITextureRegion?> DriveIcon
        {
            get => _driveIcon;
            set => SetAndNotify(ref _driveIcon, value);
        }

        public StyleProperty<ITextureRegion?> FileIcon
        {
            get => _fileIcon;
            set => SetAndNotify(ref _fileIcon, value);
        }

        public StyleProperty<FileSystemDialogMode> Mode
        {
            get => _dialogMode;
            set => SetAndNotify(ref _dialogMode, value);
        }

        public StyleProperty<string> SelectedPath
        {
            get => _selectedPath;
            set
            {
                if (string.IsNullOrEmpty(value.Value))
                {
                    value = new StyleProperty<string>(Environment.CurrentDirectory);
                }

                if (SetAndNotify(ref _selectedPath, value))
                {
                    _ = RefreshAsync();
                }
            }
        }

        public StyleProperty<IEnumerable<FileSystemDialogFilter>?> Filter
        {
            get => _filter;
            set
            {
                if (_filter != value)
                {
                    _filter = value;
                    UpdateFilterList();
                    if (!string.IsNullOrEmpty(SelectedPath) && Directory.Exists(SelectedPath))
                        _ = RefreshAsync(); // Fire-and-forget, Async-sicher
                }
            }
        }

        public StyleProperty<int> FilterIndex
        {
            get => _filterIndex;
            set
            {
                if (_filterIndex != value)
                {
                    _filterIndex = value;
                    UpdateFilterSelection();
                    _ = RefreshAsync(); // Fire-and-forget
                }
            }
        }

        #endregion

        #region Direct Properties

        public string[]? FavouritePaths
        {
            get => _specialPaths;
            set => SetAndNotify(ref _specialPaths, value);
        }

        public string[]? BookmarkPaths
        {
            get => _favouritePaths;
            set => SetAndNotify(ref _favouritePaths, value);
        }

        protected Button BackButton => _backButton;
        protected Button ForwardButton => _forwardButton;
        protected Button ParentButton => _parentButton;
        protected Button RefreshButton => _refreshButton;

        public string EntryName
        {
            get => _pathField.Text.Value ?? string.Empty;
            internal set => _pathField.Text = new StyleProperty<string?>(value);
        }

        public string SelectedFullPath => Path.Combine(SelectedPath, EntryName);

        public bool AutoAddFilterExtension { get; set; }

        #endregion

        #region Helpers

        private void BuildUI()
        {
            _contentLayout.ColumnsProportions.Add(new Proportion(ProportionType.Pixel, 180));
            _contentLayout.ColumnsProportions.Add(new Proportion(ProportionType.Pixel, 5));
            _contentLayout.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
            _contentLayout.RowsProportions.Add(new Proportion(ProportionType.Auto));
            _contentLayout.RowsProportions.Add(new Proportion(ProportionType.Fill));
            _contentLayout.RowsProportions.Add(new Proportion(ProportionType.Auto));
            _contentLayout.RowsProportions.Add(new Proportion(ProportionType.Auto));
            _contentLayout.Id = "_contentLayout";
            _contentLayout.RowSpacing = 5;
            _contentLayout.ColumnSpacing = 0;

            BuildExpanders();
            BuildToolbar();
            BuildFields();

            // FS List
            _fsList.VerticalAlignment = Visuals.VerticalAlignment.Stretch;
            _fsList.HorizontalAlignment = Visuals.HorizontalAlignment.Stretch;
            Grid.SetRow(_fsList, 1);
            Grid.SetColumn(_fsList, 2);
            _contentLayout.Widgets.Add(_fsList);

            var bottomContainer = CreateBottomContainer();
            Grid.SetRow(bottomContainer, 3);
            Grid.SetColumn(bottomContainer, 2);
            _contentLayout.Widgets.Add(bottomContainer);

            Width = 600;
            Height = 400;
            Content = _contentLayout;
        }

        private void BuildExpanders()
        {
            // Expander Scroll
            _expanderScrollViewer.VerticalAlignment = Visuals.VerticalAlignment.Stretch;
            _expanderScrollViewer.HorizontalAlignment = Visuals.HorizontalAlignment.Stretch;
            Grid.SetRow(_expanderScrollViewer, 1);
            Grid.SetRowSpan(_expanderScrollViewer, 3);
            Grid.SetColumn(_expanderScrollViewer, 0);
            _contentLayout.Widgets.Add(_expanderScrollViewer);

            var expanderLayout = new VerticalStackPanel();
            expanderLayout.Spacing = 5;
            _expanderScrollViewer.Content = expanderLayout;

            // Favourites
            var favouritePathsExpander = new Foldout()
            {
                Header = "Favourites",
                IsExpanded = true,
                Content = _favouritePathList,
                HorizontalAlignment = Visuals.HorizontalAlignment.Stretch,
                ContentHorizontalAlignment = Visuals.HorizontalAlignment.Stretch
            };
            expanderLayout.Widgets.Add(favouritePathsExpander);

            // Drives
            var drivesExpander = new Foldout()
            {
                Header = "Drives",
                IsExpanded = true,
                Content = _driveList,
                HorizontalAlignment = Visuals.HorizontalAlignment.Stretch,
                ContentHorizontalAlignment = Visuals.HorizontalAlignment.Stretch
            };
            expanderLayout.Widgets.Add(drivesExpander);

            // Favourites
            var specialPathsExpander = new Foldout()
            {
                Header = "Specials",
                IsExpanded = true,
                Content = _specialPathList,
                HorizontalAlignment = Visuals.HorizontalAlignment.Stretch,
                ContentHorizontalAlignment = Visuals.HorizontalAlignment.Stretch
            };
            expanderLayout.Widgets.Add(specialPathsExpander);
        }

        private void BuildFields()
        {
            var fieldPanel = new VerticalStackPanel();
            fieldPanel.Spacing = 5;
            Grid.SetRow(fieldPanel, 2);
            Grid.SetColumn(fieldPanel, 2);
            _contentLayout.Widgets.Add(fieldPanel);

            // Name (File/Folder)
            _nameLabel.Text = "Name:";
            _nameField.Id = "_nameField";
            _nameField.HorizontalAlignment = Visuals.HorizontalAlignment.Stretch;
            AddField(fieldPanel, new(), _nameLabel, _nameField);

            // Filter
            _filterLabel.Text = "File Type:";
            _filterField.Id = "_filterField";
            _filterField.HorizontalAlignment = Visuals.HorizontalAlignment.Stretch;
            _filterField.SelectionChanged += async (s, e) =>
            {
                _filterIndex = _filterField.SelectedIndex.Value;
                await RefreshAsync();
            };
            AddField(fieldPanel, _fileFilterPanel, _filterLabel, _filterField);
        }

        private void AddField(VerticalStackPanel panel, HorizontalStackPanel rowPanel, Label label, Widget field)
        {
            label.Width = 100;
            rowPanel.Children.Add(label);

            StackPanel.SetProportionType(field, ProportionType.Fill);
            rowPanel.Children.Add(field);

            panel.Widgets.Add(rowPanel);
        }

        private void BuildToolbar()
        {
            // Buttons
            _backButton.HorizontalAlignment = Visuals.HorizontalAlignment.Center;
            _backButton.VerticalAlignment = Visuals.VerticalAlignment.Center;
            _backButton.Id = "_backButton";
            _backButton.Content = _backIcon;
            _backButton.Click += ButtonBack_Click;

            _forwardButton.HorizontalAlignment = Visuals.HorizontalAlignment.Center;
            _forwardButton.VerticalAlignment = Visuals.VerticalAlignment.Center;
            _forwardButton.Id = "_forwardButton";
            _forwardButton.Content = _forwardIcon;
            _forwardButton.Click += ButtonForward_Click;

            _parentButton.HorizontalAlignment = Visuals.HorizontalAlignment.Center;
            _parentButton.VerticalAlignment = Visuals.VerticalAlignment.Center;
            _parentButton.Id = "_parentButton";
            _parentButton.Content = _parentIcon;
            _parentButton.Click += ButtonParent_Click;

            _refreshButton.HorizontalAlignment = Visuals.HorizontalAlignment.Center;
            _refreshButton.VerticalAlignment = Visuals.VerticalAlignment.Center;
            _refreshButton.Id = "_refreshButton";
            _refreshButton.Content = _refreshIcon;
            _refreshButton.Click += ButtonRefresh_Click;

            _pathField.IsReadOnly = true;
            _pathField.VerticalAlignment = Visuals.VerticalAlignment.Center;
            _pathField.Id = "_pathField";
            StackPanel.SetProportionType(_pathField, ProportionType.Fill);

            // Toolbar
            var toolBarPanel = new HorizontalStackPanel();
            toolBarPanel.Spacing = 4;
            toolBarPanel.Widgets.Add(_backButton);
            toolBarPanel.Widgets.Add(_forwardButton);
            toolBarPanel.Widgets.Add(_pathField);
            toolBarPanel.Widgets.Add(_parentButton);
            toolBarPanel.Widgets.Add(_refreshButton);
            Grid.SetRow(toolBarPanel, 0);
            Grid.SetColumn(toolBarPanel, 0);
            Grid.SetColumnSpan(toolBarPanel, 3);
            _contentLayout.Widgets.Add(toolBarPanel);
        }

        private void ButtonRefresh_Click(object? sender, EventArgs e)
        {
        }

        private void ButtonParent_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedPath)) return;

            var parent = Path.GetDirectoryName(SelectedPath);
            if (!string.IsNullOrEmpty(parent))
                SelectedPath = parent;
        }

        private void ButtonForward_Click(object? sender, EventArgs e)
        {
            if (_historyPosition >= _history.Count - 1) return;
            ++_historyPosition;
            SetFolder(_history[_historyPosition], false);
        }

        private void ButtonBack_Click(object? sender, EventArgs e)
        {
            if (_historyPosition <= 0) return;
            --_historyPosition;
            SetFolder(_history[_historyPosition], false);
        }

        private async Task LoadDrivesAsync()
        {
            _driveList.Widgets.Clear();

            DriveInfo[] drives = DriveInfo.GetDrives();

            // Parallel alle DriveItems erzeugen
            var driveTasks = drives.Select(drive => Task.Run(() =>
            {
                try
                {
                    string driveName = drive.Name;
                    if (!string.IsNullOrEmpty(drive.VolumeLabel))
                        driveName += $" ({drive.VolumeLabel})";

                    return CreatePlaceItem(driveName, drive.RootDirectory.FullName, true);
                }
                catch
                {
                    return null;
                }
            })).ToArray();

            var results = await Task.WhenAll(driveTasks);

            // UI-Thread: Items hinzufügen
            foreach (var item in results)
            {
                if (item != null)
                    _driveList.Widgets.Add(item);
            }
        }

        private async Task LoadFavouritesAsync(List<string> favouritePaths)
        {
            _favouritePathList.Widgets.Clear();

            // Parallel alle Favourites erzeugen
            var favTasks = favouritePaths.Select(path => Task.Run(() =>
            {
                try
                {
                    string name = Path.GetFileName(path);
                    return CreatePlaceItem(name, path, false);
                }
                catch
                {
                    return null;
                }
            })).ToArray();

            var results = await Task.WhenAll(favTasks);

            // UI-Thread: Items hinzufügen
            foreach (var item in results)
            {
                if (item != null)
                    _favouritePathList.Widgets.Add(item);
            }
        }

        private async Task LoadFileSystem(string path)
        {
            _fsList.Widgets.Clear();
            var entries = new List<string>();

            await Task.Run(() =>
            {
                try
                {
                    entries.AddRange(FilterDirectories(path));
                    entries.AddRange(FilterFiles(path));
                }
                catch { }
            });

            foreach (var entry in entries)
            {
                var item = new HorizontalStackPanel
                {
                    Spacing = ImageTextSpacing,
                    Tag = entry
                };
                var image = new Image
                {
                    Visual = Directory.Exists(entry) ? FolderIcon : FileIcon
                };
                item.Widgets.Add(image);
                item.Widgets.Add(new Label { Text = Path.GetFileName(entry) });
                _fsList.Widgets.Add(item);
            }
        }

        private static Widget CreatePlaceItem(string text, string path, bool isDrive)
        {
            var button = new Button();
            button.Width = null;
            button.Height = null;
            button.HorizontalAlignment = Visuals.HorizontalAlignment.Stretch;
            button.ContentHorizontalAlignment = Visuals.HorizontalAlignment.Stretch;
            var panel = new HorizontalStackPanel
            {
                Spacing = ImageTextSpacing,
                Tag = new PathInfo(path, isDrive)
            };
            panel.Widgets.Add(new Image());
            panel.Widgets.Add(new Label { Text = text });
            button.Content = panel;
            return button;
        }

        private void SetFolder(string? value, bool storeInHistory)
        {
            if (!Directory.Exists(value)) return;

            _pathField.Text = value;

            if (!storeInHistory) return;

            while (_history.Count > 0 && _historyPosition < _history.Count - 1)
                _history.RemoveAt(_history.Count - 1);

            _history.Add(value);
            _historyPosition = _history.Count - 1;
        }

        #endregion

        protected virtual Container CreateBottomContainer() => new HorizontalStackPanel();

        protected async override void OnLoaded()
        {
            base.OnLoaded();
            await LoadFavouritesAsync(FavouritePaths?.ToList() ?? new List<string>());
            await LoadDrivesAsync();
            _expanderScrollViewer.InvalidateArrange();
        }

        public async Task RefreshAsync()
        {
            _pathField.Text = _selectedPath.Value;
            SetFolder(_selectedPath.Value, true);
            await LoadFileSystem(_selectedPath.Value ?? "");
        }

        protected internal override bool CanCloseByConfirm()
        {
            if (_confirmed)
            {
                _confirmed = false;
                return true;
            }

            if (_dialogMode.Value == FileSystemDialogMode.SaveFile)
            {
                var fileName = EntryName;

                // Filter-Erweiterung anhängen
                if (AutoAddFilterExtension && _filter.Value != null && _filter.Value.Any())
                {
                    var firstExt = _filter.Value
                        .SelectMany(f => f.Pattern?.Split(';', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
                        .FirstOrDefault(p => p.StartsWith("*."));
                    if (!string.IsNullOrEmpty(firstExt) && !fileName.EndsWith(firstExt[1..], StringComparison.OrdinalIgnoreCase))
                    {
                        fileName += firstExt[1..]; // ".ext"
                    }
                }

                var filePath = Path.Combine(SelectedPath, fileName);

                if (File.Exists(filePath))
                {
                    MessageDialog.Show(Desktop,
                        $"File '{fileName}' already exists. Do you want to replace it?",
                        MessageDialog.MessageButtons.YesNo,
                        result =>
                        {
                            if (result == MessageDialog.MessageResults.Yes)
                            {
                                EntryName = fileName;
                                _confirmed = true;
                                //PerformConfirm(); // Wiederholen
                            }
                        });
                    return false;
                }

                EntryName = fileName;
                return true;
            }
            else
            {
                return true;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            _backButton.Click -= ButtonBack_Click;
            _forwardButton.Click -= ButtonForward_Click;
            _parentButton.Click -= ButtonParent_Click;
            _refreshButton.Click -= ButtonRefresh_Click;
        }

        #region Filter 

        protected virtual IEnumerable<string> FilterDirectories(string currentPath)
        {
            foreach (var folder in Directory.EnumerateDirectories(currentPath))
            {
                var dirInfo = new DirectoryInfo(folder);
                var attr = dirInfo.Attributes;

                if ((attr & (FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReparsePoint)) != 0)
                    continue;

                yield return folder;
            }
        }

        protected virtual IEnumerable<string> FilterFiles(string currentPath)
        {
            IEnumerable<string> patterns;

            if (_filter.Value == null || !_filter.Value.Any() || _filterField.SelectedIndex < 0)
            {
                patterns = new[] { "*.*" };
            }
            else
            {
                var currentFilter = _filter.Value.ElementAtOrDefault(_filterField.SelectedIndex.Value);
                patterns = currentFilter?.Pattern?
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    ?? new[] { "*.*" };
            }

            foreach (var pattern in patterns)
            {
                foreach (var file in Directory.EnumerateFiles(currentPath, pattern))
                {
                    var attr = File.GetAttributes(file);

                    if ((attr & (FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReparsePoint)) != 0)
                        continue;

                    if (_dialogMode.Value == FileSystemDialogMode.SaveFile && (attr & FileAttributes.ReadOnly) != 0)
                        continue;

                    yield return file;
                }
            }
        }

        private void UpdateFilterList()
        {
            _filterField.Items.Clear();

            if (_filter.Value == null || !_filter.Value.Any())
            {
                _fileFilterPanel.IsVisible = false;
                return;
            }

            _fileFilterPanel.IsVisible = true;

            foreach (var filter in _filter.Value)
            {
                var itemLabel = new Label
                {
                    Text = filter.Title,
                    Tag = filter.Pattern
                };
                _filterField.Items.Add(itemLabel);
            }

            UpdateFilterSelection();
        }

        private void UpdateFilterSelection()
        {
            if (_filterField.Items.Count == 0)
            {
                _filterField.SelectedIndex = -1;
                return;
            }

            if (_filterIndex >= 0 && _filterIndex < _filterField.Items.Count)
                _filterField.SelectedIndex = _filterIndex;
            else
                _filterField.SelectedIndex = 0;
        }

        #endregion

        #region Style

        public override void ApplyFromStyle(Style? style)
        {
            base.ApplyFromStyle(style?.ResolveApplyFor() ?? style);

            var backButtonStyle = style?.FindStyle(typeof(Button), "back-button");
            _backButton.ApplyFromStyle(backButtonStyle);
            _backIcon.ApplyFromStyle(backButtonStyle?.FindStyle(typeof(Image)));

            var forwardButtonStyle = style?.FindStyle(typeof(Button), "forward-button");
            _forwardButton.ApplyFromStyle(forwardButtonStyle);
            _forwardIcon.ApplyFromStyle(forwardButtonStyle?.FindStyle(typeof(Image)));

            var parentButtonStyle = style?.FindStyle(typeof(Button), "parent-button");
            _parentButton.ApplyFromStyle(parentButtonStyle);
            _parentIcon.ApplyFromStyle(parentButtonStyle?.FindStyle(typeof(Image)));

            var refreshButtonStyle = style?.FindStyle(typeof(Button), "refresh-button");
            _refreshButton.ApplyFromStyle(refreshButtonStyle);
            _refreshIcon.ApplyFromStyle(refreshButtonStyle?.FindStyle(typeof(Image)));

            // Button-spezifische Properties
            style?.Apply(this, (target, sheet, property, value) =>
            {
                switch (property)
                {
                    case nameof(DriveIcon):
                        target.DriveIcon = target.DriveIcon.Override(sheet.FindRegion(value.RawValue));
                        break;
                    case nameof(FolderIcon):
                        target.FolderIcon = target.FolderIcon.Override(sheet.FindRegion(value.RawValue));
                        break;
                    case nameof(FileIcon):
                        target.FileIcon = target.FileIcon.Override(sheet.FindRegion(value.RawValue));
                        break;
                }
            });
        }

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not FileSystemDialog source)
                return;
        }

        protected override ElementBase CreateCloneInstance()
        {
            return new FileSystemDialog();
        }

        #endregion
    }
}
