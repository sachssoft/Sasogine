//using System;
//using System.Collections;
//using System.Collections.Generic;

//namespace Sachssoft.Sasogine.Presentation.Deterlite.Layouts
//{
//    public class FrameCollection : IList<FrameBase>
//    {
//        private readonly FrameBase _owner;

//        public FrameCollection(FrameBase owner)
//        {
//            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
//        }

//        public FrameBase this[int index]
//        {
//            get => _owner.GetChild(index);
//            set
//            {
//                if (value == null) throw new ArgumentNullException(nameof(value));
//                _owner.RemoveChild(_owner.GetChild(index));
//                _owner.AddChild(value);
//            }
//        }

//        public int Count => _owner.ChildCount;

//        public bool IsReadOnly => false;

//        public void Add(FrameBase item)
//        {
//            if (item == null) throw new ArgumentNullException(nameof(item));
//            _owner.AddChild(item);
//        }

//        public void Clear()
//        {
//            while (_owner.ChildCount > 0)
//            {
//                _owner.RemoveChild(_owner.GetChild(0));
//            }
//        }

//        public bool Contains(FrameBase item)
//        {
//            if (item == null) return false;
//            for (int i = 0; i < Count; i++)
//            {
//                if (ReferenceEquals(_owner.GetChild(i), item)) return true;
//            }
//            return false;
//        }

//        public void CopyTo(FrameBase[] array, int arrayIndex)
//        {
//            if (array == null) throw new ArgumentNullException(nameof(array));
//            if (arrayIndex < 0 || arrayIndex + Count > array.Length)
//                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

//            for (int i = 0; i < Count; i++)
//            {
//                array[arrayIndex + i] = _owner.GetChild(i);
//            }
//        }

//        public IEnumerator<FrameBase> GetEnumerator()
//        {
//            for (int i = 0; i < Count; i++)
//                yield return _owner.GetChild(i);
//        }

//        public int IndexOf(FrameBase item)
//        {
//            if (item == null) return -1;
//            for (int i = 0; i < Count; i++)
//            {
//                if (ReferenceEquals(_owner.GetChild(i), item)) return i;
//            }
//            return -1;
//        }

//        public void Insert(int index, FrameBase item)
//        {
//            if (item == null) throw new ArgumentNullException(nameof(item));
//            // Kein echter Insert, nur Add, da Owner intern Liste kontrolliert
//            _owner.AddChild(item);
//        }

//        public bool Remove(FrameBase item)
//        {
//            if (item == null) return false;
//            if (Contains(item))
//            {
//                _owner.RemoveChild(item);
//                return true;
//            }
//            return false;
//        }

//        public void RemoveAt(int index)
//        {
//            _owner.RemoveChild(_owner.GetChild(index));
//        }

//        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
//    }
//}