using Sachssoft.Sasogine.Presentation.Deterlite.Rendering;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Controls
{
    public class Widget : FrameBase
    {
        public bool IsEnabled { get; set; } = true;

        public bool IsFocusable { get; set; } = true; 

        public bool IsFocused { get; set; }

        public IBrush? FocusBrush { get; set; }

        public IBrush? DisabledBackgroundBrush { get; set; }

        public IBrush? DisabledForegroundBrush { get; set; }

        public IBrush? HoveredBackgroundBrush { get; set; }

        public IBrush? HoveredForegroundBrush { get; set; }
    }
}
