using System;
using System.Windows.Input;

namespace Sachssoft.Sasogine.Inputs
{
    /// <summary>
    /// High-performance FIFO command queue using a fixed-size ring buffer.
    /// Designed for frame-based execution scenarios such as game loops,
    /// editor tools, or Immediate Mode GUI systems like ImGui.
    /// Commands are executed deterministically in the order they were queued.
    /// </summary>
    // Performante FIFO Command-Queue basierend auf einem Ringpuffer
    // Geeignet für Frame-basierte Systeme (GameLoop, Editor, ImGui)
    // Commands werden deterministisch in der Reihenfolge ausgeführt
    // Verwendet ein fixes Array → keine dynamischen Reallocations
    public sealed class CommandQueue
    {
        /// <summary>
        /// Internal buffer storing queued commands.
        /// </summary>
        // Internes Array zum Speichern der Commands
        private readonly ICommand[] _buffer;

        /// <summary>
        /// Index of the first command in the queue.
        /// </summary>
        // Index des ersten Elements der Queue
        private int _head;

        /// <summary>
        /// Index of the next free slot in the queue.
        /// </summary>
        // Index der nächsten freien Position
        private int _tail;

        /// <summary>
        /// Initializes a new command queue with a fixed capacity.
        /// </summary>
        /// <param name="capacity">Maximum number of commands the queue can hold.</param>
        // Initialisiert die Queue mit einer festen Kapazität
        // capacity = maximale Anzahl gleichzeitig gespeicherter Commands
        public CommandQueue(int capacity = 256)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _buffer = new ICommand[capacity];
            _head = 0;
            _tail = 0;
        }

        /// <summary>
        /// Gets whether the queue currently contains no commands.
        /// </summary>
        // Gibt true zurück wenn keine Commands vorhanden sind
        public bool IsEmpty => _head == _tail;

        /// <summary>
        /// Gets the number of commands currently stored in the queue.
        /// </summary>
        // Anzahl der aktuell gespeicherten Commands
        public int Count => (_tail - _head + _buffer.Length) % _buffer.Length;

        /// <summary>
        /// Adds a command to the end of the queue.
        /// </summary>
        /// <param name="command">Command to enqueue.</param>
        // Fügt einen Command am Ende der Queue hinzu
        // Bei Überlauf wird eine Exception geworfen
        public void Enqueue(ICommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            int next = (_tail + 1) % _buffer.Length;

            // Prüfen ob Queue voll ist
            if (next == _head)
                throw new InvalidOperationException("CommandQueue capacity exceeded.");

            _buffer[_tail] = command;
            _tail = next;
        }

        /// <summary>
        /// Removes and returns the command at the front of the queue.
        /// </summary>
        /// <returns>The dequeued command.</returns>
        // Entfernt und gibt den ersten Command der Queue zurück
        public ICommand Dequeue()
        {
            if (IsEmpty)
                throw new InvalidOperationException("CommandQueue is empty.");

            var cmd = _buffer[_head];

            // Referenz freigeben → GC kann Objekt sammeln
            _buffer[_head] = null!;

            _head = (_head + 1) % _buffer.Length;

            return cmd;
        }

        /// <summary>
        /// Executes all queued commands in FIFO order.
        /// </summary>
        /// <param name="parameter">
        /// Optional parameter passed to CanExecute and Execute.
        /// </param>
        // Führt alle Commands der Queue der Reihe nach aus
        // Jeder Command wird nur ausgeführt wenn CanExecute true liefert
        // Nach der Ausführung ist die Queue leer
        public void ExecuteAll(object? parameter = null)
        {
            while (!IsEmpty)
            {
                var command = Dequeue();

                if (command != null && command.CanExecute(parameter))
                    command.Execute(parameter);
            }
        }

        /// <summary>
        /// Removes all commands from the queue without executing them.
        /// </summary>
        // Leert die Queue ohne Commands auszuführen
        public void Clear()
        {
            while (!IsEmpty)
            {
                _buffer[_head] = null!;
                _head = (_head + 1) % _buffer.Length;
            }

            _head = 0;
            _tail = 0;
        }
    }
}