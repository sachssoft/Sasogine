namespace Sachssoft.Sasogine.Surface.Controls
{
    public enum TextWrapMode
    {
        None,       // width = null → SingleLine
        WordWrap,   // width = ActualBounds.Width
        Ellipsis,   // width = ActualBounds.Width + AutoEllipsisMethod.End
    }
}
