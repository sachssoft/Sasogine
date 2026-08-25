using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Input
{

    public sealed class AxisInput<TEnum> where TEnum : unmanaged, Enum
    {
        private readonly Vector2[] _values;

        public AxisInput(int max_enum_value = 511)
        {
            _values = new Vector2[max_enum_value + 1];
        }

        public void Set(TEnum axis, Vector2 value)
        {
            int index = Unsafe.As<TEnum, int>(ref axis);
            if ((uint)index < _values.Length)
                _values[index] = value;
        }

        public Vector2 Get(TEnum axis)
        {
            int index = Unsafe.As<TEnum, int>(ref axis);
            return (uint)index < _values.Length ? _values[index] : Vector2.Zero;
        }

        public void ForEachNonZero(Action<TEnum, Vector2> action)
        {
            for (int i = 0; i < _values.Length; i++)
            {
                if (_values[i] != Vector2.Zero)
                {
                    TEnum axis = Unsafe.As<int, TEnum>(ref i);
                    action(axis, _values[i]);
                }
            }
        }

        public void Clear()
        {
            Array.Clear(_values, 0, _values.Length);
        }
    }
}