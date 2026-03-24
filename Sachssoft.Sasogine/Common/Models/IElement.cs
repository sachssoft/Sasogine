using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Represents a generic element in the engine.
    /// Provides access to its definition, identity, class, data context, and lifecycle methods.
    /// </summary>
    // Stellt ein generisches Element in der Engine dar.
    // Bietet Zugriff auf Definition, ID, Klasse, DataContext und Lebenszyklus-Methoden.
    public interface IElement
    {
        /// <summary>
        /// Gets the definition instance that configures this element.
        /// </summary>
        // Liefert die Definition, die dieses Element konfiguriert.
        IElementDefinition Definition { get; }

        /// <summary>
        /// Indicates whether the element has been loaded.
        /// </summary>
        // Gibt an, ob das Element geladen wurde.
        bool IsLoaded { get; }

        /// <summary>
        /// Gets the unique identifier of this element.
        /// </summary>
        // Liefert die eindeutige ID des Elements.
        string? Id { get; }

        /// <summary>
        /// Gets the class/type name of this element.
        /// </summary>
        // Liefert den Klassennamen/Typ des Elements.
        string? Class { get; }

        /// <summary>
        /// Gets or sets a custom data context associated with this element.
        /// </summary>
        // Liefert oder setzt einen benutzerdefinierten DataContext für dieses Element.
        object? DataContext { get; set; }

        /// <summary>
        /// Loads the element synchronously.
        /// </summary>
        // Lädt das Element synchron.
        void Load();

        /// <summary>
        /// Loads the element asynchronously.
        /// </summary>
        // Lädt das Element asynchron.
        Task LoadAsync();

        /// <summary>
        /// Unloads the element, releasing any resources.
        /// </summary>
        // Entlädt das Element und gibt Ressourcen frei.
        void Unload();
    }
}