using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Behaviors;
using System;

namespace Sachssoft.Sasogine.Surface.Controls.Primitives
{
    public abstract class PopupBase : ElementBase
    {
        private Widget? _content;
        private Desktop? _popupHost;

        public event EventHandler<CancellableEventArgs<Widget>>? Opening;
        public event EventHandler? Opened;
        public event EventHandler<CancellableEventArgs<Widget>>? Closing;
        public event EventHandler? Closed;

        public PopupBase()
        {
        }

        #region Direct Properties

        //public bool IsOpen => _popupHost?._popup == this; 
        public bool IsOpen => _popupHost != null && _popupHost.Popups.Contains(this);

        public Widget? Owner { get; internal set; }

        #endregion

        public static PopupBase Create(Widget content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            return new InternalPopup(content);
        }

        protected abstract Widget CreateContent();

        internal Widget Content
        {
            get
            {
                if (_content == null)
                {
                    _content = CreateContent()
                         ?? throw new InvalidOperationException();

                    _content.ArrangeUpdated += Content_ArrangeUpdated; 
                }

                return _content;
            }
        }

        private void Content_ArrangeUpdated(object? sender, EventArgs e)
        {
            OnContentLayoutUpdated(e);
        }

        protected virtual void OnContentLayoutUpdated(EventArgs e)
        {
        }

        public void Open(Widget widget, Point position)
        {
            Open(widget.Desktop, position);
        }

        public void Open(Desktop host, Point position)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            _popupHost = host;
            host.OpenPopup(this, position);
        }

        public void Close()
        {
            _popupHost?.ClosePopup();
            _popupHost = null;
        }

        internal protected virtual void OnOpening(CancellableEventArgs<Widget> e)
        {
            Opening?.Invoke(this, e);
        }

        internal protected virtual void OnOpened(EventArgs e)
        {
            Opened?.Invoke(this, e);
        }

        internal protected virtual void OnClosing(CancellableEventArgs<Widget> e)
        {
            Closing?.Invoke(this, e);
        }

        internal protected virtual void OnClosed(EventArgs e)
        {
            Closed?.Invoke(this, e);
        }

        private class InternalPopup : PopupBase
        {
            private readonly Widget _content0;

            public InternalPopup(Widget content)
            {
                _content0 = content;
            }

            protected override ElementBase CreateCloneInstance()
            {
                return new InternalPopup(_content0);
            }

            protected override Widget CreateContent()
            {
                return _content0;
            }
        }
    }
}
