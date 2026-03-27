using Sachssoft.Sasogine.Interactions;
using Sachssoft.Sasogine.Presentation;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Interactions;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class ToolbarButton : ToolbarItemBase, IGroupedCheckable, ICommandSource
    {
        private StyleProperty<ITextureRegion?> _icon = new StyleProperty<ITextureRegion?>(null, isUserSet: false);
        private StyleProperty<object?> _header = new StyleProperty<object?>(null, isUserSet: false);
        private StyleProperty<object?> _toolTip = new StyleProperty<object?>(null, isUserSet: false);
        private StyleProperty<bool> _isChecked = new StyleProperty<bool>(false, isUserSet: false);
        private StyleProperty<ToolbarButtonType> _type = new StyleProperty<ToolbarButtonType>(ToolbarButtonType.Regular, isUserSet: false);
        private StyleProperty<string?> _groupName = new StyleProperty<string?>(null, isUserSet: false);
        private StyleProperty<ICommand?> _command = new StyleProperty<ICommand?>(null, isUserSet: false);
        private StyleProperty<object?> _commandParameter = new StyleProperty<object?>(null, isUserSet: false);

        private ButtonBase? _button;
        private bool _isLoaded;
        private SceneBase? _commandOwner;

        #region Events

        public event EventHandler? IsCheckedChanged;

        #endregion

        public ToolbarButton()
        {
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
            set => SetAndNotify(ref _header, value);
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
                    SyncCheckableState();
            }
        }

        public StyleProperty<ToolbarButtonType> Type
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
            set => SetAndNotify(ref _groupName, value);
        }

        public StyleProperty<ICommand?> Command
        {
            get => _command;
            set
            {
                if (SetAndNotify(ref _command, value))
                {
                    if (_button != null)
                        _button.Command = _command;
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
                    if (_button != null)
                        _button.CommandParameter = _commandParameter;
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
                    if (_button != null)
                        _button.CommandOwner = _commandOwner;
                }
            }
        }

        #endregion

        #region Style

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not ToolbarButton source)
                return;

            Icon = source.Icon;
            ToolTip = source.ToolTip;
            IsChecked = source.IsChecked;
            Type = source.Type;
            GroupName = source.GroupName;
            Command = source.Command;
            CommandParameter = source.CommandParameter;
        }

        protected override ElementBase CreateCloneInstance()
        {
            return new ToolbarButton();
        }

        #endregion

        #region Helpers

        protected override void OnIsEnabledChanged(EventArgs e)
        {
            base.OnIsEnabledChanged(e);
            UpdateButton();
        }

        private void SyncCheckableState()
        {
            if (_button is ICheckable checkable)
            {
                checkable.IsCheckedChanged -= Checkable_IsCheckedChanged;
                checkable.IsChecked = _isChecked.Value;
                checkable.IsCheckedChanged += Checkable_IsCheckedChanged;
            }
        }

        private ButtonBase BuildPresenter()
        {
            UnloadPresenter();

            _button = _type.Value switch
            {
                ToolbarButtonType.Regular => new Button(),
                ToolbarButtonType.Toggle => new ToggleButton(),
                ToolbarButtonType.Choice => new ChoiceButton(),
                _ => throw new NotSupportedException($"Unsupported ToolbarButtonType: {_type.Value}")
            };

            _button.Content = new Image
            {
                HorizontalAlignment = Visuals.HorizontalAlignment.Center,
                VerticalAlignment = Visuals.VerticalAlignment.Center
            };

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

                if (_button is ICheckable checkable)
                {
                    checkable.IsChecked = _isChecked.Value;
                }

                if (_button is IGroupedCheckable groupable)
                {
                    groupable.GroupName = _groupName.Value;
                }

                var icon = _button.Content.Value as Image;

                if (icon != null)
                {
                    icon.Visual = Icon;
                    icon.HoveredVisual = Icon;
                }

                if (Owner is Toolbar toolbar)
                {
                    _button.Width = toolbar.IconWidth.Value + toolbar.IconMargin.Value.Left + toolbar.IconMargin.Value.Right;
                    _button.Height = toolbar.IconHeight.Value + toolbar.IconMargin.Value.Top + toolbar.IconMargin.Value.Bottom;

                    if (icon != null)
                    {
                        icon.Width = toolbar.IconWidth.Value;
                        icon.Height = toolbar.IconHeight.Value;
                    }
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
