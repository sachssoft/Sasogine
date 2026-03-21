using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Interactions;
using Sachssoft.Sasogine.Presentation;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Interactions;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Windows.Input;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class MenuItem : MenuItemBase, IGroupedCheckable, ICommandSource
    {
        private StyleProperty<ITextureRegion?> _icon = new StyleProperty<ITextureRegion?>(null, isUserSet: false);
        private StyleProperty<object?> _header = new StyleProperty<object?>(null, isUserSet: false);
        private StyleProperty<object?> _toolTip = new StyleProperty<object?>(null, isUserSet: false);
        private StyleProperty<bool> _isChecked = new StyleProperty<bool>(false, isUserSet: false);
        private StyleProperty<MenuItemButtonType> _type = new StyleProperty<MenuItemButtonType>(MenuItemButtonType.Regular, isUserSet: false);
        private StyleProperty<string?> _groupName = new StyleProperty<string?>(null, isUserSet: false);
        private StyleProperty<Shortcut> _shortcut = new StyleProperty<Shortcut>(Sasogine.Interactions.Shortcut.Default, isUserSet: false);
        private StyleProperty<string?> _shortcutText = new StyleProperty<string?>(null, isUserSet: false);
        private StyleProperty<ICommand?> _command = new StyleProperty<ICommand?>(null, isUserSet: false);
        private StyleProperty<object?> _commandParameter = new StyleProperty<object?>(null, isUserSet: false);

        private MenuItemPresenter _button = new MenuItemPresenter();
        private SceneBase? _commandOwner;

        #region Events

        public event EventHandler? IsCheckedChanged;

        #endregion

        public MenuItem()
        {
        }

        public MenuItem(object? header) : this(null, header, null)
        {
        }

        public MenuItem(object? header, ICommand? command) : this(null, header, command)
        {
        }

        public MenuItem(string? id, object? header, ICommand? command)
        {
            Id = id;
            Header = new StyleProperty<object?>(header);
            Command = new StyleProperty<ICommand?>(command);
        }

        #region Style Properties

        public StyleProperty<ITextureRegion?> Icon
        {
            get => _icon;
            set
            {
                if (SetAndNotify(ref _icon, value))
                {
                    UpdateButton();
                }
            }
        }

        public StyleProperty<object?> Header
        {
            get => _header;
            set
            {
                if (SetAndNotify(ref _header, value))
                {
                    UpdateButton();
                }
            }
        }

        public StyleProperty<object?> ToolTip
        {
            get => _toolTip;
            set => SetAndNotify(ref _toolTip, value);
        }

        public StyleProperty<bool> IsChecked
        {
            get => _isChecked;
            set
            {
                if (SetAndNotify(ref _isChecked, value))
                {
                    SyncCheckableState();
                }
            }
        }

        public StyleProperty<MenuItemButtonType> Type
        {
            get => _type;
            set
            {
                if (SetAndNotify(ref _type, value))
                {
                    _button = BuildPresenter();
                    InvalidatePresenter();
                }
            }
        }

        public StyleProperty<string?> GroupName
        {
            get => _groupName;
            set
            {
                if (SetAndNotify(ref _groupName, value))
                {
                    UpdateButton();
                }
            }
        }

        public StyleProperty<Shortcut> Shortcut
        {
            get => _shortcut;
            set
            {
                if (SetAndNotify(ref _shortcut, value))
                {
                    UpdateButton();
                }
            }
        }

        public StyleProperty<string?> ShortcutText
        {
            get => _shortcutText;
            set
            {
                if (SetAndNotify(ref _shortcutText, value))
                {
                    UpdateButton();
                }
            }
        }

        public StyleProperty<ICommand?> Command
        {
            get => _command;
            set
            {
                if (SetAndNotify(ref _command, value))
                {
                    UpdateButton();
                }
            }
        }

        public StyleProperty<object?> CommandParameter
        {
            get => _commandParameter;
            set
            {
                if (SetAndNotify(ref _commandParameter, value))
                {
                    UpdateButton();
                }
            }
        }

        #endregion

        #region Direct Properties

        internal protected override Widget Presenter
        {
            get => _button ?? BuildPresenter();
        }

        public SceneBase? CommandOwner
        {
            get => _commandOwner;
            set
            {
                if (SetAndNotify(ref _commandOwner, value))
                {
                    UpdateButton();
                }
            }
        }

        #endregion

        #region Style

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not MenuItem source)
                return;

            Icon = source.Icon;
            ToolTip = source.ToolTip;
            IsChecked = source.IsChecked;
            Type = source.Type;
            GroupName = source.GroupName;
            Shortcut = source.Shortcut;
            ShortcutText = source.ShortcutText;
            Command = source.Command;
            CommandParameter = source.CommandParameter;
        }

        protected override ElementBase CreateCloneInstance()
        {
            return new MenuItem();
        }

        #endregion

        #region Helpers

        private void SyncCheckableState()
        {
            if (_button is ICheckable checkable)
            {
                checkable.IsCheckedChanged -= Checkable_IsCheckedChanged;
                checkable.IsChecked = _isChecked.Value;
                checkable.IsCheckedChanged += Checkable_IsCheckedChanged;
            }
        }

        private MenuItemPresenter BuildPresenter()
        {
            UnloadPresenter();
            UpdateButton();

            if (_button is ICheckable checkable)
            {
                checkable.IsCheckedChanged += Checkable_IsCheckedChanged;
            }

            return _button;
        }

        private void UpdateButton()
        {
            if (_button != null)
            {
                _button.IsEnabled = IsEnabled.Value;
                _button.Command = _command;
                _button.CommandParameter = _commandParameter.Value;
                _button.CommandOwner = _commandOwner;
                _button.ShortcutText = _shortcutText.Value;

                if (_button is ICheckable checkable)
                {
                    checkable.IsChecked = _isChecked.Value;
                }

                if (_button is IGroupedCheckable groupable)
                {
                    groupable.GroupName = _groupName.Value;
                }

                _button.Icon = new StyleProperty<ITextureRegion?>(_icon.Value);
                _button.Header = new StyleProperty<object?>(_header.Value);
                _button.Margin = new StyleProperty<Visuals.Thickness>(new Visuals.Thickness(5));

                if (Owner is Menu menu)
                {
                    _button.IconWidth = menu.IconWidth.Value + menu.IconMargin.Value.Left + menu.IconMargin.Value.Right;
                    _button.IconHeight = menu.IconHeight.Value + menu.IconMargin.Value.Top + menu.IconMargin.Value.Bottom;
                    _button.IconMargin = menu.IconMargin.Value;
                }
            }

        }

        private void Checkable_IsCheckedChanged(object? sender, EventArgs e)
        {
            if (sender is ICheckable checkable)
            {
                _isChecked = new StyleProperty<bool>(checkable.IsChecked);
                IsCheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void UnloadPresenter()
        {
            if (_button is ICheckable checkable)
                checkable.IsCheckedChanged -= Checkable_IsCheckedChanged;

            _button = null;
        }

        protected override void OnOwnerPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnOwnerPropertyChanged(e);
            UpdateButton();
        }

        #endregion

        protected override void OnIsEnabledChanged(EventArgs e)
        {
            base.OnIsEnabledChanged(e);
            UpdateButton();
        }

        #region IGroupedCheckable

        string? IGroupedCheckable.GroupName
        {
            get => _groupName.Value;
            set => _groupName = new StyleProperty<string?>(value);
        }

        #endregion

        #region IChecked

        bool ICheckable.IsChecked
        {
            get => _isChecked.Value;
            set => _isChecked = new StyleProperty<bool>(value);
        }

        #endregion

        #region ICommandSource

        ICommand? ICommandSource.Command
        {
            get => _command.Value;
            set => _command = new StyleProperty<ICommand?>(value);
        }

        object? ICommandSource.CommandParameter
        {
            get => _commandParameter.Value;
            set => _commandParameter = new StyleProperty<object?>(value);
        }

        object? ICommandSource.CommandOwner
        {
            get => CommandOwner;
            set => CommandOwner = (SceneBase?)value;
        }

        #endregion
    }
}
