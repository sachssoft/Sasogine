namespace sachssoft.Sasogine.Surface.Visuals.Styles
{
    public class ImageButtonStyle : ButtonStyle
    {
        public PressableImageStyle ImageStyle { get; set; }

        public ImageButtonStyle()
        {
        }

        public ImageButtonStyle(ImageButtonStyle style) : base(style)
        {
            ImageStyle = style.ImageStyle != null ? new PressableImageStyle(style.ImageStyle) : null;
        }

        public ImageButtonStyle(ButtonStyle buttonStyle) : base(buttonStyle)
        {
        }

        public override WidgetStyle Clone()
        {
            return new ImageButtonStyle(this);
        }
    }
}
