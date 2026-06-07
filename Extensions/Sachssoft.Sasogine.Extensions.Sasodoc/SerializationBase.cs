using Sachssoft.Sasodoc;

namespace Sachssoft.Sasogine.Extensions.Sasodoc
{
    public abstract class SerializationBase<T> : ISerialization
        where T : class
    {
        public abstract T Deserialize(FormatReaderBase reader);

        public abstract void Serialize(T source, FormatWriterBase writer);

        object ISerialization.Deserialize(FormatReaderBase reader)
        {
            var result = Deserialize(reader);

            if (result is null)
                throw new InvalidOperationException(
                    $"Deserializer returned null for type {typeof(T).Name}. This is not allowed.");

            return result;
        }

        void ISerialization.Serialize(object source, FormatWriterBase writer)
        {
            if (source is not T typed)
            {
                throw new InvalidOperationException(
                    $"Serialization type mismatch. Expected {typeof(T).Name}, got {source.GetType().Name}");
            }

            Serialize(typed, writer);
        }
    }
}