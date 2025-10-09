using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Interactions
{
    public static class InteractionExtensions
    {
        public static void Bind<TInteractionEnum>(this KeyboardInteractionManager manager, Keys button, Interaction<TInteractionEnum> interaction, TInteractionEnum value) where TInteractionEnum : unmanaged, Enum
        {
            manager.Add(button,
                () => interaction.Press(value),
                () => interaction.Release(value)
            );
        }

        public static void BindCombination<TInteractionEnum>(this KeyboardInteractionManager manager, IEnumerable<Keys> buttons, Interaction<TInteractionEnum> interaction, TInteractionEnum value) where TInteractionEnum : unmanaged, Enum
        {
            manager.AddCombination(buttons,
                () => interaction.Press(value),
                () => interaction.Release(value)
            );
        }

        public static void BindSequence<TInteractionEnum>(this KeyboardInteractionManager manager, IList<Keys> sequences, Interaction<TInteractionEnum> interaction, TInteractionEnum value, TimeSpan? timeout = null) where TInteractionEnum : unmanaged, Enum
        {
            manager.AddSequence(sequences,
                () => interaction.Press(value),
                timeout
            );
        }
    }
}
