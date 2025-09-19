using System;

namespace Sachssoft.Sasogine.Diagnostics;

public interface IDebugDisplay
{
    void SendDebugText(object? sender, string? text);

    void Update(GameFrameContext context);
}
