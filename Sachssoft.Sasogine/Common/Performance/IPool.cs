namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Generic interface for object pools.
    /// Provides methods to acquire and release objects efficiently.
    /// </summary>
    // Generisches Interface für Objekt-Pools.
    // Bietet Methoden zum effizienten Abrufen und Freigeben von Objekten.
    public interface IPool<T> where T : class
    {
        /// <summary>
        /// Gets an object from the pool, or creates a new one if the pool is empty.
        /// </summary>
        // Holt ein Objekt aus dem Pool oder erstellt ein neues, falls leer.
        T Get();

        /// <summary>
        /// Returns an object back to the pool for reuse.
        /// </summary>
        // Gibt ein Objekt zurück in den Pool zur Wiederverwendung.
        void Release(T obj);

        /// <summary>
        /// Number of objects currently in the pool.
        /// </summary>
        // Anzahl der aktuell im Pool vorhandenen Objekte.
        int Count { get; }
    }
}
