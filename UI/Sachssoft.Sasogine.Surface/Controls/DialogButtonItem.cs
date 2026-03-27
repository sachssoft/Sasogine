using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Styles;
using System;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class DialogButtonItem : ElementBase
    {
        private StyleProperty<object?> _content = new StyleProperty<object?>(null, false);
        private StyleProperty<int> _result = new StyleProperty<int>(0, false);

        public DialogButtonItem()
        {
        }

        public DialogButtonItem(object? content, int result)
        {
            Content = new StyleProperty<object?>(content, false);
            Result = new StyleProperty<int>(result, false);
        }

        #region Style Properties

        public StyleProperty<object?> Content
        {
            get => _content;
            set => SetAndNotify(ref _content, value);
        }

        public StyleProperty<int> Result
        {
            get => _result;
            set => SetAndNotify(ref _result, value);
        }

        #endregion

        #region Style

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not DialogButtonItem source)
                return;

            Result = source.Result;

            if (source.Content.Value is ICloneable cloneable)
            {
                Content = cloneable.Clone();
            }
            else
            {
                Content = source.Content;
            }
        }

        protected override ElementBase CreateCloneInstance()
        {
            return new DialogButtonItem();
        }

        #endregion
    }
}
