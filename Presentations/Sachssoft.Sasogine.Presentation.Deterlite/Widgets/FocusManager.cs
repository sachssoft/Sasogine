//namespace Sachssoft.Sasogine.Deterlite.Basic
//{
//    public ref struct FocusManager
//    {
//        private IFocusable[] _focusables;
//        private int _currentFocus;

//        public void Register(IFocusable control, int index)
//        {
//            _focusables[index] = control;
//        }

//        public void Update(InputState input)
//        {
//            if (_focusables.Length == 0) return;

//            // Beispiel Keyboard / Gamepad Navigation
//            if (input.NextFocus) MoveNext();
//            if (input.PrevFocus) MovePrev();

//            // Alle Controls: Focus setzen
//            for (int i = 0; i < _focusables.Length; i++)
//            {
//                _focusables[i].SetFocus(i == _currentFocus);
//            }
//        }

//        private void MoveNext()
//        {
//            _currentFocus = (_currentFocus + 1) % _focusables.Length;
//        }

//        private void MovePrev()
//        {
//            _currentFocus = (_currentFocus - 1 + _focusables.Length) % _focusables.Length;
//        }
//    }
//}
