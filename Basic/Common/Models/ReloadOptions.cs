using System;
using System.Collections.Generic;
using System.Text;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Specifies which parts of an engine object should be reloaded.
    /// </summary>
    /// <remarks>
    // Legt fest, welche Bereiche eines Engine-Objekts beim Reload
    // aktualisiert werden sollen.
    // </remarks>
    [Flags]
    public enum ReloadOptions
    {
        /// <summary>
        /// No reload operation.
        /// </summary>
        // Keine Reload-Aktion.
        None = 0,

        /// <summary>
        /// Updates the runtime object from its definition.
        /// </summary>
        // Übernimmt Änderungen aus der Definition in das bestehende Runtime-Objekt.
        // Beispielsweise geänderte Eigenschaften oder Parameter.
        UpdateDefinition = 1,

        /// <summary>
        /// Recreates the runtime state.
        /// </summary>
        // Erstellt den internen Runtime-Zustand neu.
        // Beispielsweise Komponenten, Caches oder interne Objekte.
        RecreateRuntime = 2,

        /// <summary>
        /// Reloads referenced resources.
        /// </summary>
        // Lädt referenzierte Ressourcen neu.
        // Beispielsweise Assets, Texturen, Modelle oder Sounds.
        ReloadResources = 4,

        /// <summary>
        /// Performs a complete reload.
        /// </summary>
        // Führt einen vollständigen Reload durch.
        // Aktualisiert die Definition, erzeugt den Runtime-Zustand neu
        // und lädt alle referenzierten Ressourcen erneut.
        Full = UpdateDefinition | RecreateRuntime | ReloadResources
    }
}
