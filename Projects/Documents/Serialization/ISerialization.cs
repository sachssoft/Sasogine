using Sachssoft.Sasodoc;

namespace Sachssoft.Sasogine.Documents.Serialization
{
    /// <summary>
    /// Defines methods for serializing and deserializing an object using Sasodoc format readers and writers.
    /// </summary>
    public interface ISerialization
    {
        /// <summary>
        /// Serializes the specified source object.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        /// <param name="writer">The writer that receives the serialized values.</param>
        void Serialize(object source, FormatWriterBase writer);

        /// <summary>
        /// Deserializes values into the specified target object.
        /// </summary>
        /// <param name="target">The object to populate with deserialized values.</param>
        /// <param name="reader">The reader that provides the serialized values.</param>
        void Deserialize(object target, FormatReaderBase reader);
    }
}
