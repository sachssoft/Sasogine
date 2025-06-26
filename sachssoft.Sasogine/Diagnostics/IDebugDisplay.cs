using System;

namespace sachssoft.Sasogine.Diagnostics;

public interface IDebugDisplay
{
    void SendDebugText(object? sender, string? text);

    void Update(GameContext context);
}
