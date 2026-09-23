using System;
using System.Collections.Generic;
using System.Text;

namespace Sachssoft.Engine.Examples;

static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        using var game = new FirstStepGLApp();
        game.Run();
    }
}