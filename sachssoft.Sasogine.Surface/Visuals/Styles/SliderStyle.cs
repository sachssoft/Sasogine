namespace sachssoft.Sasogine.Surface.Visuals.Styles
{
    public class SliderStyle : WidgetStyle
    {
        public ImageButtonStyle KnobStyle { get; set; }

        public SliderStyle()
        {
        }

        public SliderStyle(SliderStyle style) : base(style)
        {
            KnobStyle = style.KnobStyle != null ? new ImageButtonStyle(style.KnobStyle) : null;
        }

        public override WidgetStyle Clone()
        {
            return new SliderStyle(this);
        }
    }
}
