using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Input
{

    [DebuggerDisplay("Interaction[{EnumTypeName}]")]
    public unsafe sealed class Interaction<TEnum> : IInteraction where TEnum : unmanaged, Enum
    {
        //private const int MaxEnumValue = 511; // oder 1023, je nach Bitbreite
        private readonly int _max_enum_value;
        private readonly ulong[] _bitmask;
        private readonly ulong[] _last_bitmask;

        public delegate void InteractionChangedHandler(TEnum interaction, bool pressed);
        public event InteractionChangedHandler? InteractionChanged;

        public Interaction(int max_enum_value = 511)
        {
            _max_enum_value = max_enum_value;
            int enum_count = _max_enum_value + 1;
            int ulongCount = (enum_count + 63) / 64;
            _bitmask = new ulong[ulongCount];
            _last_bitmask = new ulong[ulongCount];
        }

        public void Press(TEnum interaction)
        {
            if (TryGetIndexAndBit(interaction, _max_enum_value, out int group, out int bit))
                _bitmask[group] |= (1UL << bit);
        }

        public void Press(params TEnum[] interactions)
        {
            for (int i = 0; i < interactions.Length; i++)
                Press(interactions[i]);
        }

        public void Release(TEnum interaction)
        {
            if (TryGetIndexAndBit(interaction, _max_enum_value, out int group, out int bit))
                _bitmask[group] &= ~(1UL << bit);
        }

        public void Release(params TEnum[] interactions)
        {
            for (int i = 0; i < interactions.Length; i++)
                Release(interactions[i]);
        }

        public bool IsPressed(TEnum interaction)
        {
            return TryGetIndexAndBit(interaction, _max_enum_value, out int group, out int bit)
                && (_bitmask[group] & (1UL << bit)) != 0;
        }

        public bool WasJustPressed(TEnum interaction)
        {
            if (!TryGetIndexAndBit(interaction, _max_enum_value, out int group, out int bit)) return false;
            ulong mask = 1UL << bit;
            return (_bitmask[group] & mask) != 0 && (_last_bitmask[group] & mask) == 0;
        }

        public bool WasJustReleased(TEnum interaction)
        {
            if (!TryGetIndexAndBit(interaction, _max_enum_value, out int group, out int bit)) return false;
            ulong mask = 1UL << bit;
            return (_bitmask[group] & mask) == 0 && (_last_bitmask[group] & mask) != 0;
        }

        public void Update()
        {
            for (int i = 0; i < _bitmask.Length; i++)
                _last_bitmask[i] = _bitmask[i];
        }

        public void Clear()
        {
            for (int group = 0; group < _bitmask.Length; group++)
            {
                ulong bits = _bitmask[group];
                if (bits == 0) continue;

                for (int bit = 0; bit < 64; bit++)
                {
                    ulong mask = 1UL << bit;
                    if ((bits & mask) != 0)
                    {
                        _bitmask[group] &= ~mask;
                        int index = group * 64 + bit;
                        TEnum interaction = Unsafe.As<int, TEnum>(ref index);
                        InteractionChanged?.Invoke(interaction, false);
                    }
                }
            }
        }

        public void ForEachPressed(Action<TEnum> action)
        {
            for (int group = 0; group < _bitmask.Length; group++)
            {
                ulong bits = _bitmask[group];
                if (bits == 0) continue;

                for (int bit = 0; bit < 64; bit++)
                {
                    if ((bits & (1UL << bit)) != 0)
                    {
                        int index = group * 64 + bit;
                        TEnum interaction = Unsafe.As<int, TEnum>(ref index);
                        action(interaction);
                    }
                }
            }
        }

        public void ForEachJustReleased(Action<TEnum> action)
        {
            for (int group = 0; group < _bitmask.Length; group++)
            {
                ulong current = _bitmask[group];
                ulong previous = _last_bitmask[group];
                ulong changed = previous & ~current;

                if (changed == 0) continue;

                for (int bit = 0; bit < 64; bit++)
                {
                    if ((changed & (1UL << bit)) != 0)
                    {
                        int index = group * 64 + bit;
                        TEnum interaction = Unsafe.As<int, TEnum>(ref index);
                        action(interaction);
                    }
                }
            }
        }

        public void ForEachJustPressed(Action<TEnum> action)
        {
            for (int group = 0; group < _bitmask.Length; group++)
            {
                ulong current = _bitmask[group];
                ulong previous = _last_bitmask[group];
                ulong changed = current & ~previous;

                if (changed == 0) continue;

                for (int bit = 0; bit < 64; bit++)
                {
                    if ((changed & (1UL << bit)) != 0)
                    {
                        int index = group * 64 + bit;
                        TEnum interaction = Unsafe.As<int, TEnum>(ref index);
                        action(interaction);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int UnsafeIndex(TEnum value)
        {
            return *(int*)(&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryGetIndexAndBit(TEnum interaction, int max_enum_value, out int group, out int bit)
        {
            int index = UnsafeIndex(interaction);
            if (index < 0 || index > max_enum_value)
            {
#if DEBUG
                Debug.WriteLine($"[Interaction] WARN: Enum-Wert {interaction} ({index}) außerhalb des gültigen Bereichs 0..{max_enum_value}");
#endif
                group = bit = -1;
                return false;
            }

            group = index / 64;
            bit = index % 64;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TEnum ToEnum(ulong value)
        {
            int v = (int)value; // truncate auf int
            return Unsafe.As<int, TEnum>(ref v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong ToUInt64(TEnum value)
            => (ulong)UnsafeIndex(value);

        #region IInteraction

        void IInteraction.Press(ulong interaction)
            => Press(ToEnum(interaction));

        void IInteraction.Press(params ulong[] interactions)
        {
            for (int i = 0; i < interactions.Length; i++)
                Press(ToEnum(interactions[i]));
        }

        void IInteraction.Release(ulong interaction)
            => Release(ToEnum(interaction));

        void IInteraction.Release(params ulong[] interactions)
        {
            for (int i = 0; i < interactions.Length; i++)
                Release(ToEnum(interactions[i]));
        }

        bool IInteraction.IsPressed(ulong interaction)
            => IsPressed(ToEnum(interaction));

        bool IInteraction.WasJustPressed(ulong interaction)
            => WasJustPressed(ToEnum(interaction));

        bool IInteraction.WasJustReleased(ulong interaction)
            => WasJustReleased(ToEnum(interaction));

        void IInteraction.ForEachPressed(Action<ulong> action)
            => ForEachPressed(x => action(ToUInt64(x)));

        void IInteraction.ForEachJustReleased(Action<ulong> action)
            => ForEachJustReleased(x => action(ToUInt64(x)));

        void IInteraction.ForEachJustPressed(Action<ulong> action)
            => ForEachJustPressed(x => action(ToUInt64(x)));

        #endregion

    }
}