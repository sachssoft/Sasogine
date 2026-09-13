using Sachssoft.Sasodoc;
using System;

namespace Sachssoft.Sasogine.Markup.Serialization
{
    /// <summary>
    /// Provides a strongly typed base class for markup serialization handlers.
    /// </summary>
    /// <typeparam name="T">
    /// The object type handled by the serializer.
    /// </typeparam>
    public abstract class SerializationBase<T> : ISerialization
        where T : class
    {
        /// <summary>
        /// Serializes the specified strongly typed source object.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        /// <param name="writer">The writer that receives the serialized values.</param>
        public abstract void Serialize(T source, FormatWriterBase writer);

        /// <summary>
        /// Deserializes values into the specified strongly typed target object.
        /// </summary>
        /// <param name="target">The object to populate with deserialized values.</param>
        /// <param name="reader">The reader that provides the serialized values.</param>
        public abstract void Deserialize(T target, FormatReaderBase reader);

        void ISerialization.Serialize(object source, FormatWriterBase writer)
        {
            ArgumentNullException.ThrowIfNull(writer);

            if (source is null)
                throw new ArgumentNullException(nameof(source),
                    $"Cannot serialize null source for type {typeof(T).FullName}.");

            if (source is not T typed)
                throw new InvalidOperationException(
                    $"Serialize type mismatch. Expected {typeof(T).FullName}, got {source.GetType().FullName}.");

            Serialize(typed, writer);
        }

        void ISerialization.Deserialize(object target, FormatReaderBase reader)
        {
            ArgumentNullException.ThrowIfNull(reader);

            if (target is null)
                throw new ArgumentNullException(nameof(target),
                    $"Cannot deserialize into null target for type {typeof(T).FullName}.");

            if (target is not T typed)
                throw new InvalidOperationException(
                    $"Deserialize type mismatch. Expected {typeof(T).FullName}, got {target.GetType().FullName}.");

            Deserialize(typed, reader);
        }
    }
}