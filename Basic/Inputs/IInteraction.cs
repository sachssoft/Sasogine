using System;

namespace Sachssoft.Sasogine.Input
{
    public interface IInteraction
    {
        void Press(ulong interaction);
        void Press(params ulong[] interactions);
        void Release(ulong interaction);
        void Release(params ulong[] interactions);
        bool IsPressed(ulong interaction);
        bool WasJustPressed(ulong interaction);
        bool WasJustReleased(ulong interaction);
        void Update();
        void Clear();
        void ForEachPressed(Action<ulong> action);
        void ForEachJustReleased(Action<ulong> action);
        void ForEachJustPressed(Action<ulong> action);
    }
}
