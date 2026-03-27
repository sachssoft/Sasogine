namespace Sachssoft.Sasogine.Surface.Controls.Primitives
{
    public class StateIndicator : Image
    {
        protected override bool UseOverBackground
        {
            get
            {
                if (Parent == null)
                {
                    return IsMouseInside;
                }

                return Parent.IsMouseInside;
            }
        }
    }
}