using System;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.World
{
    /// <summary>
    /// Basisschnittstelle für alle Nodes im Spiel/Editor.
    /// <para>Status:</para> Dauerhafter Zustand der Node (Intact / Warning / Error)
    /// <para>ActivityState:</para> Temporäre Aktion oder Arbeit der Node (Idle / Active)
    /// </summary>
    // z.B. Tiles, Sprites, Scripts, Audio, usw...

    // Wenn mit Update dann IRuntimeComponent implementieren
    // und auch mit Zeichen dann IDrawableRuntimeComponent implementieren
    public interface IEntity
    {
        // Events
        event EventHandler? Loaded;
        event EventHandler? Unloaded;
        event EventHandler? StatusChanged;
        event EventHandler? ActivityStateChanged;

        // Identifikation
        string? Id { get; }
        string? Class { get; }

        //
        object? DataContext { get; }

        // Zustand
        EntityIntegrity Integrity { get; }               // Intact / Warning / Error
        ActivityState ActivityState { get; }     // Idle / Active

        // Lifecycle
        void Load();
        Task LoadAsync();
        void Unload();
    }
}