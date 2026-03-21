using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Behaviors;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public partial class Desktop
    {
        #region ToolTip

        public Widget? Tooltip { get; private set; }

        public void HideTooltip()
        {
            if (Tooltip == null)
            {
                return;
            }

            Widgets.Remove(Tooltip);
            Tooltip.IsVisible = false;
            Tooltip = null;
        }

        public void ShowTooltip(Widget widget, Point position)
        {
            if (widget.TooltipPresenter == null)
            {
                return;
            }

            HideTooltip();
            Tooltip = UIEnvironment.TooltipCreator(widget);
            if (Tooltip == null)
            {
                return;
            }

            FixOverWidgetPosition(Tooltip, position);

            Tooltip.IsVisible = true;
            Widgets.Add(Tooltip);
        }

        #endregion
    }
}
