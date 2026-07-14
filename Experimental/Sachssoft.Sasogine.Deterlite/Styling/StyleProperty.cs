using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sachssoft.Sasogine.Presentation.Styling
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct StyleProperty<T>
    {
        private readonly T _value;
        private readonly bool _isUserSet;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StyleProperty(T value)
        {
            _value = value;
            _isUserSet = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StyleProperty(T value, bool isUserSet)
        {
            _value = value;
            _isUserSet = isUserSet;
        }

        public T Value => _value;

        // Der Stil/Style kann nur Eigenschaften übernehmen, die nie gesetzt wurden
        public bool IsUserSet => _isUserSet;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StyleProperty<T>(T value)
            => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T(in StyleProperty<T> setter)
            => setter._value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(StyleProperty<T> left, StyleProperty<T> right)
        {
            if (!left._isUserSet && !right._isUserSet)
                return true;
            if (left._isUserSet != right._isUserSet)
                return false;
            return Equals(left._value, right._value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(StyleProperty<T> left, StyleProperty<T> right)
            => !(left == right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StyleProperty<T> Override(T value)
        {
            return _isUserSet ? this : new StyleProperty<T>(value, isUserSet: false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StyleProperty<T> Apply(T value)
        {
            return new StyleProperty<T>(value, isUserSet: _isUserSet);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj)
        {
            if (obj is not StyleProperty<T> other)
                return false;

            // Beides vergleichen: Wert und ob user gesetzt
            return _isUserSet == other._isUserSet &&
                   Equals(_value, other._value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            // Kombiniere _isUserSet und _value in den Hashcode
            int hash = 17;
            hash = hash * 31 + _isUserSet.GetHashCode();
            hash = hash * 31 + (_value?.GetHashCode() ?? 0);
            return hash;
        }

        public override string ToString()
            => _isUserSet ? _value?.ToString() ?? "null" : ("unset; " + _value?.ToString());

    }
}
