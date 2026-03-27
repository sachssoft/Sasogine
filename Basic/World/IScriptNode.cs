using System;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.World
{
    /// <summary>
    /// Node, die Skriptlogik enthält oder Aktionen ausführt.
    /// Kann von Editor und Runtime genutzt werden.
    /// </summary>
    public interface IScriptNode : INode
    {
        /// <summary>
        /// Führt das Skript oder die Logik einmal synchron aus.
        /// </summary>
        /// <param name="args">Optionale Argumente für das Skript</param>
        void Execute(params object?[] args);

        /// <summary>
        /// Führt das Skript oder die Logik asynchron aus.
        /// </summary>
        /// <param name="args">Optionale Argumente für das Skript</param>
        Task ExecuteAsync(params object?[] args);

        /// <summary>
        /// True, solange das Skript aktiv läuft
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Event, das ausgelöst wird, wenn das Skript beendet oder unterbrochen wird
        /// </summary>
        event EventHandler? ExecutionFinished;
    }
}