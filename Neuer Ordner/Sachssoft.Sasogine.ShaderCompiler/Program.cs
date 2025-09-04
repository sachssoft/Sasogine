using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

class Program
{
    static void Main()
    {
        // Pfade anpassen
        string fxPath = @"D:\Projekte\VarietyGolf\Sachssoft.VarietyGolf.Shared\Content\effects\color_shader.fx";
        string mgfxoPath = @"D:\Projekte\VarietyGolf\Sachssoft.VarietyGolf.Shared\Content\effects\color_shader.mgfxo";

        // HLSL-Code einlesen
        string fxSource = File.ReadAllText(fxPath);

        // GraphicsDevice vorbereiten
        using (var graphics = new GraphicsDeviceManagerDummy())
        {
            var graphicsDevice = graphics.GraphicsDevice;

            // FX kompilieren
            try
            {
                var effectData = Effect.CompileEffectFromSource(fxSource, null, null, CompilerOptions.None, TargetPlatform.DesktopGL);
                if (effectData.Success)
                {
                    File.WriteAllBytes(mgfxoPath, effectData.GetEffectCode());
                    Console.WriteLine("Erfolgreich kompiliert: " + mgfxoPath);
                }
                else
                {
                    Console.WriteLine("Fehler beim Kompilieren:");
                    Console.WriteLine(effectData.ErrorsAndWarnings);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex);
            }
        }
    }
}

// Dummy GraphicsDeviceManager für Konsolen-App
class GraphicsDeviceManagerDummy : IDisposable
{
    public GraphicsDevice GraphicsDevice { get; private set; }

    public GraphicsDeviceManagerDummy()
    {
        var gd = new GraphicsDevice(
            GraphicsAdapter.DefaultAdapter,
            GraphicsProfile.HiDef,
            new PresentationParameters
            {
                BackBufferWidth = 1,
                BackBufferHeight = 1,
                DeviceWindowHandle = IntPtr.Zero,
                DepthStencilFormat = DepthFormat.None,
                IsFullScreen = false
            });

        GraphicsDevice = gd;
    }

    public void Dispose()
    {
        GraphicsDevice.Dispose();
    }
}
