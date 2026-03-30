using Sachssoft.Sasogine.Surface.Basic;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class ToolbarCustomItem : ToolbarItemBase
    {
        private ITemplateFactory<Widget>? _widgetFactory;

        public ToolbarCustomItem()
        {
        }

        public ITemplateFactory<Widget>? WidgetFactory
        {
            get => _widgetFactory;
            set => SetAndNotify(ref _widgetFactory, value);
        }

        protected internal override Widget Presenter => throw new System.NotImplementedException();

        #region Style

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not ToolbarCustomItem source)
                return;

            // ...
        }

        protected override ElementBase CreateCloneInstance()
        {
            return new ToolbarCustomItem();
        }

        #endregion
    }
}
