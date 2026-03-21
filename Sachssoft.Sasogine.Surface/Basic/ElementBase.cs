using Sachssoft.Sasogine.Inspection;
using Sachssoft.Sasogine.Surface.Styles;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Surface.Basic
{
    public abstract class ElementBase : NotifyObject, 
        IItemWithId, ICloneable,
        INotifyAttachedPropertyChanged, INotifyPropertyChanged, INotifyPropertyChanging
    {
        private StyleProperty<bool> _isVisible = new StyleProperty<bool>(true, isUserSet: false);
        private StyleProperty<bool> _isEnabled = new StyleProperty<bool>(true, isUserSet: false);
        private StyleProperty<object?> _tag = new StyleProperty<object?>(null, isUserSet: false);

        #region Style Properties

        public StyleProperty<bool> IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (SetAndNotify(ref _isEnabled, value))
                {
                    OnIsEnabledChanged(EventArgs.Empty);
                }
            }
        }

        public StyleProperty<bool> IsVisible
        {
            get => _isVisible;
            set
            {
                if (SetAndNotify(ref _isVisible, value))
                {
                    OnIsVisibleChanged(EventArgs.Empty);
                }
            }
        }

        public StyleProperty<object?> Tag
        {
            get => _tag;
            set => SetAndNotify(ref _tag, value);
        }

        #endregion

        #region Clone

        public virtual void ApplyFrom(ElementBase other)
        {
            IsEnabled = other.IsEnabled;
            IsVisible = other.IsVisible;
        }

        protected abstract ElementBase CreateCloneInstance();

        public ElementBase Clone()
        {
            var clonedElement = CreateCloneInstance();
            clonedElement.ApplyFrom(this);
            return clonedElement;
        }

        public TElement Clone<TElement>() where TElement : ElementBase
        {
            var r = Clone();
            if (r is not TElement result)
                throw new InvalidCastException($"CloneOrSelf is not of type {typeof(TElement).Name}.");

            return result;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        #region Helpers

        protected virtual void OnIsEnabledChanged(EventArgs e) { }

        protected virtual void OnIsVisibleChanged(EventArgs e) { }

        #endregion

        #region Obsolete

        [Obsolete]
        public readonly Dictionary<int, object> AttachedPropertiesValues = new Dictionary<int, object>();

        [Obsolete]
        public virtual void OnAttachedPropertyChanged(BaseAttachedPropertyInfo propertyInfo) { }

        #endregion
    }
}
