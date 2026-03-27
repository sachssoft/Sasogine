using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;

namespace Sachssoft.Sasogine.Surface.Styles
{
    public static class DefaultStyle
    {
        #region Old
        public static readonly int FieldHeight = 25;
        public static readonly int BoxWidth = 25;
        public static readonly int BoxHeight = 25;

        public static readonly IBrush ContainerBrush = new SolidColorBrush("#585858");
        public static readonly IBrush UnderlyingLabelBrush = new SolidColorBrush("#505050");
        public static readonly IBrush BoxBrush = new SolidColorBrush("#303030");
        public static readonly IBrush InteractionBrush = new SolidColorBrush("#404040");
        public static readonly IBrush EditableBoxBrush = new SolidColorBrush("#202020");
        public static readonly IBrush HighlightBrush = new SolidColorBrush("#6fa8dc");


        #endregion

        public static readonly Font TextFont = Stylesheet.Current.DefaultFont;
        public static readonly Font TitleFont = Stylesheet.Current.DefaultFont;

        public static readonly Color TextColor = Color.White;
        public static readonly Color DisabledTextColor = Color.Gray;
        public static readonly Color OverTextColor = Color.LightGray;
        public static readonly Color PressedTextColor = Color.LightGray;
    }
}
