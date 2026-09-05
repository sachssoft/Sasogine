using Sachssoft.Sasogine.Components;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Provides interaction state tracking for values of the specified enumeration type.
    /// </summary>
    /// <typeparam name="TEnum">
    /// The enumeration type used to identify interactions.
    /// </typeparam>
    [DebuggerDisplay("Interaction[{EnumTypeName}]")]
    public unsafe sealed class Interaction<TEnum> :
        IInteraction,
        IUpdatableComponentService
        where TEnum : unmanaged, Enum
    {
        private readonly int _maxEnumValue;
        private readonly ulong[] _bitMask;
        private readonly ulong[] _lastBitMask;

        /// <summary>
        /// Represents a method that handles a change to an interaction state.
        /// </summary>
        /// <param name="interaction">
        /// The interaction whose state changed.
        /// </param>
        /// <param name="pressed">
        /// <see langword="true"/> if the interaction is pressed;
        /// otherwise, <see langword="false"/>.
        /// </param>
        public delegate void InteractionChangedHandler(
            TEnum interaction,
            bool pressed);

        /// <summary>
        /// Occurs when an interaction state changes.
        /// </summary>
        public event InteractionChangedHandler? InteractionChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Interaction{TEnum}"/> class.
        /// </summary>
        /// <param name="maxEnumValue">
        /// The maximum numeric enumeration value supported by this interaction.
        /// </param>
        public Interaction(int maxEnumValue = 511)
        {
            _maxEnumValue = maxEnumValue;

            int enumCount = _maxEnumValue + 1;
            int ulongCount = (enumCount + 63) / 64;

            _bitMask = new ulong[ulongCount];
            _lastBitMask = new ulong[ulongCount];
        }

        /// <summary>
        /// Sets the specified interaction to the pressed state.
        /// </summary>
        /// <param name="interaction">
        /// The interaction to press.
        /// </param>
        public void Press(TEnum interaction)
        {
            if (TryGetIndexAndBit(
                interaction,
                _maxEnumValue,
                out int group,
                out int bit))
            {
                _bitMask[group] |= 1UL << bit;
            }
        }

        /// <summary>
        /// Sets the specified interactions to the pressed state.
        /// </summary>
        /// <param name="interactions">
        /// The interactions to press.
        /// </param>
        public void Press(params TEnum[] interactions)
        {
            for (int i = 0; i < interactions.Length; i++)
                Press(interactions[i]);
        }

        /// <summary>
        /// Sets the specified interaction to the released state.
        /// </summary>
        /// <param name="interaction">
        /// The interaction to release.
        /// </param>
        public void Release(TEnum interaction)
        {
            if (TryGetIndexAndBit(
                interaction,
                _maxEnumValue,
                out int group,
                out int bit))
            {
                _bitMask[group] &= ~(1UL << bit);
            }
        }

        /// <summary>
        /// Sets the specified interactions to the released state.
        /// </summary>
        /// <param name="interactions">
        /// The interactions to release.
        /// </param>
        public void Release(params TEnum[] interactions)
        {
            for (int i = 0; i < interactions.Length; i++)
                Release(interactions[i]);
        }

        /// <summary>
        /// Determines whether the specified interaction is currently pressed.
        /// </summary>
        /// <param name="interaction">
        /// The interaction to query.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the interaction is currently pressed;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsPressed(TEnum interaction)
        {
            return TryGetIndexAndBit(
                interaction,
                _maxEnumValue,
                out int group,
                out int bit)
                && (_bitMask[group] & (1UL << bit)) != 0;
        }

        /// <summary>
        /// Determines whether the specified interaction was just pressed.
        /// </summary>
        /// <param name="interaction">
        /// The interaction to query.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the interaction was just pressed;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool WasJustPressed(TEnum interaction)
        {
            if (!TryGetIndexAndBit(
                interaction,
                _maxEnumValue,
                out int group,
                out int bit))
            {
                return false;
            }

            ulong mask = 1UL << bit;

            return (_bitMask[group] & mask) != 0
                && (_lastBitMask[group] & mask) == 0;
        }

        /// <summary>
        /// Determines whether the specified interaction was just released.
        /// </summary>
        /// <param name="interaction">
        /// The interaction to query.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the interaction was just released;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool WasJustReleased(TEnum interaction)
        {
            if (!TryGetIndexAndBit(
                interaction,
                _maxEnumValue,
                out int group,
                out int bit))
            {
                return false;
            }

            ulong mask = 1UL << bit;

            return (_bitMask[group] & mask) == 0
                && (_lastBitMask[group] & mask) != 0;
        }

        /// <summary>
        /// Updates the interaction state by storing the current state as the
        /// previous state for the next update cycle.
        /// </summary>
        /// <param name="context">
        /// Provides information about the current scene update.
        /// </param>
        public void Update(SceneUpdateContext context)
        {
            for (int i = 0; i < _bitMask.Length; i++)
                _lastBitMask[i] = _bitMask[i];
        }

        /// <summary>
        /// Releases all currently pressed interactions.
        /// </summary>
        public void Clear()
        {
            for (int group = 0; group < _bitMask.Length; group++)
            {
                ulong bits = _bitMask[group];

                if (bits == 0)
                    continue;

                for (int bit = 0; bit < 64; bit++)
                {
                    ulong mask = 1UL << bit;

                    if ((bits & mask) != 0)
                    {
                        _bitMask[group] &= ~mask;

                        int index = group * 64 + bit;
                        TEnum interaction =
                            Unsafe.As<int, TEnum>(ref index);

                        InteractionChanged?.Invoke(
                            interaction,
                            false);
                    }
                }
            }
        }

        /// <summary>
        /// Invokes the specified action for each currently pressed interaction.
        /// </summary>
        /// <param name="action">
        /// The action to invoke for each pressed interaction.
        /// </param>
        public void ForEachPressed(Action<TEnum> action)
        {
            for (int group = 0; group < _bitMask.Length; group++)
            {
                ulong bits = _bitMask[group];

                if (bits == 0)
                    continue;

                for (int bit = 0; bit < 64; bit++)
                {
                    if ((bits & (1UL << bit)) != 0)
                    {
                        int index = group * 64 + bit;
                        TEnum interaction =
                            Unsafe.As<int, TEnum>(ref index);

                        action(interaction);
                    }
                }
            }
        }

        /// <summary>
        /// Invokes the specified action for each interaction that was just released.
        /// </summary>
        /// <param name="action">
        /// The action to invoke for each just-released interaction.
        /// </param>
        public void ForEachJustReleased(Action<TEnum> action)
        {
            for (int group = 0; group < _bitMask.Length; group++)
            {
                ulong current = _bitMask[group];
                ulong previous = _lastBitMask[group];
                ulong changed = previous & ~current;

                if (changed == 0)
                    continue;

                for (int bit = 0; bit < 64; bit++)
                {
                    if ((changed & (1UL << bit)) != 0)
                    {
                        int index = group * 64 + bit;
                        TEnum interaction =
                            Unsafe.As<int, TEnum>(ref index);

                        action(interaction);
                    }
                }
            }
        }

        /// <summary>
        /// Invokes the specified action for each interaction that was just pressed.
        /// </summary>
        /// <param name="action">
        /// The action to invoke for each just-pressed interaction.
        /// </param>
        public void ForEachJustPressed(Action<TEnum> action)
        {
            for (int group = 0; group < _bitMask.Length; group++)
            {
                ulong current = _bitMask[group];
                ulong previous = _lastBitMask[group];
                ulong changed = current & ~previous;

                if (changed == 0)
                    continue;

                for (int bit = 0; bit < 64; bit++)
                {
                    if ((changed & (1UL << bit)) != 0)
                    {
                        int index = group * 64 + bit;
                        TEnum interaction =
                            Unsafe.As<int, TEnum>(ref index);

                        action(interaction);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the current interaction state as <see cref="InteractionFlags"/>.
        /// </summary>
        /// <param name="interaction">
        /// The interaction to query.
        /// </param>
        /// <returns>
        /// The flags representing the current state of the interaction.
        /// Returns <see cref="InteractionFlags.None"/> when the interaction is inactive.
        /// </returns>
        public InteractionFlags GetFlags(TEnum interaction)
        {
            InteractionFlags flags = InteractionFlags.None;

            if (IsPressed(interaction))
                flags |= InteractionFlags.IsPressed;

            if (WasJustPressed(interaction))
                flags |= InteractionFlags.WasJustPressed;

            if (WasJustReleased(interaction))
                flags |= InteractionFlags.WasJustReleased;

            return flags;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int UnsafeIndex(TEnum value)
        {
            return *(int*)(&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryGetIndexAndBit(
            TEnum interaction,
            int maxEnumValue,
            out int group,
            out int bit)
        {
            int index = UnsafeIndex(interaction);

            if (index < 0 || index > maxEnumValue)
            {
#if DEBUG
                Debug.WriteLine(
                    $"[Interaction] WARN: Enum-Wert {interaction} ({index}) außerhalb des gültigen Bereichs 0..{maxEnumValue}");
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
            int v = (int)value;
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