namespace Sachssoft.Sasogine.Surface.Controls
{
    public class ModalResult
    {

        private readonly IModalContent _owner;

        public ModalResult(IModalContent owner, int value)
        {
            _owner = owner;
            Value = value;
        }

        public TOwner GetOwner<TOwner>() where TOwner : IModalContent
        {
            return (TOwner)_owner;
        }

        public int? Value { get; }

        public bool Cancel { get; set; }


    }
}
