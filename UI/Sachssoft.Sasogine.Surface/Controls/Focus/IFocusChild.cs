namespace Sachssoft.Sasogine.Surface.Controls.Focus
{
    public interface IFocusChild
    {
        bool IsFocusable { get; set; }

        void EnterFocus();

        void LeaveFocus();
    }
}
