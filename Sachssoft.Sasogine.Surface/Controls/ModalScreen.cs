namespace Sachssoft.Sasogine.Surface.Controls
{
    internal sealed class ModalScreen : Image
    {
        public ModalScreen()
        {
            IsHitTestVisible = true;
        }

        public IModalContent? Owner { get; set; } = null;
    }
}
