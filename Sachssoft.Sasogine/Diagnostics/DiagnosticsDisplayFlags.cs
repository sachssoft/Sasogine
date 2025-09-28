using System;

namespace Sachssoft.Sasogine.Diagnostics
{
    [Flags]
    public enum DiagnosticsDisplayFlags
    {
        None = 0,                // Keine Anzeige

        FPS = 1 << 0,            // Frames per Second
        DrawCalls = 1 << 1,      // Anzahl der Draw-Calls
        MemoryUsage = 1 << 2,    // Speicherverbrauch
        CPUUsage = 1 << 3,       // CPU-Auslastung
        GPUUsage = 1 << 4,       // GPU-Auslastung
        Colliders = 1 << 5,      // Collider- und BoundingBox-Anzeige
        Collisions = 1 << 6,     // Laufende Kollisionen hervorheben
        Actors = 1 << 7,         // Positionen / BoundingBox von Actors
        CameraBounds = 1 << 8,   // Kamera-Grenzen / Sichtbereich
        Frustum = 1 << 9,        // Sichtfrustum visualisieren
        Input = 1 << 10,         // Aktuelle Eingaben
        InputMapping = 1 << 11,  // Übersicht Tasten/Controller
        Physics = 1 << 12,       // Physik-Debug (Velocity, Gravitation)
        Paths = 1 << 13,         // Pfade / Waypoints / AI-Routen
        Grid = 1 << 14,          // Raster-/Level-Overlay
        GridCells = 1 << 15,     // Zelleninformationen
        Regions = 1 << 16,       // Trigger-Zonen / Regionen
        DebugText = 1 << 17,     // Allgemeine Debug-Textanzeigen
        Particles = 1 << 18,     // Partikel-Systeme
        AI = 1 << 19,            // AI-Zustände / Entscheidungen
        Network = 1 << 20,       // Netzwerkstatus / Synchronisierung
        Animations = 1 << 21,    // Animationszustände / Frames
        Effects = 1 << 22,       // Visual Effects
        Audio = 1 << 23,         // Audio-Debug
        Layers = 1 << 24,        // Layer / Z-Index
        Shaders = 1 << 25,       // Shader-Parameter
        PerformanceGraph = 1 << 26, // Performance Graph
        Timers = 1 << 27,        // Laufzeit-Timer
        LogMessages = 1 << 28,   // Echtzeit-Logausgabe
        Events = 1 << 29,        // Spielinterne Events
        States = 1 << 30,        // Objekt/Actor States
        UI = 1 << 31,            // UI
        Elements = 1 << 32       // Elements
    }
}
