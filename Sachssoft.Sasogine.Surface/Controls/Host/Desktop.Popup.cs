using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Behaviors;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public partial class Desktop
    {
        #region Popups

        private readonly Stack<PopupBase> _popups = new Stack<PopupBase>();

        internal Stack<PopupBase> Popups => _popups;

        internal void OpenPopup(PopupBase popup, Point position)
        {
            if (popup == null)
                return;

            var eventArgs = new CancellableEventArgs<PopupBase>(popup);
            if (eventArgs.Cancel)
                return;

            // Position korrigieren
            FixOverWidgetPosition(popup.Content, position);

            // Popup öffnen
            popup.Content.IsVisible = true;
            Widgets.Add(popup.Content);

            if (popup.Content.AcceptsKeyboardFocus)
            {
                _previousKeyboardFocus = FocusedKeyboardWidget;
                FocusedKeyboardWidget = popup.Content;
            }

            popup.OnOpened(EventArgs.Empty);

            // In den Stack legen
            _popups.Push(popup);
        }

        internal void ClosePopup()
        {
            if (_popups.Count == 0)
                return;

            var popup = _popups.Pop();

            Widgets.Remove(popup.Content);
            popup.Content.IsVisible = false;

            popup.OnClosed(EventArgs.Empty);

            if (_previousKeyboardFocus != null)
            {
                FocusedKeyboardWidget = _previousKeyboardFocus;
                _previousKeyboardFocus = null;
            }
        }

        internal void CloseAllPopups()
        {
            while (_popups.Count > 0)
                ClosePopup();
        }

        #endregion
    }
}
