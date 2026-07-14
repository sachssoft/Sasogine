using Sachssoft.Sasodoc;
using System;

namespace Sachssoft.Sasogine.Extensions.Sasodoc
{
    public abstract class SerializationBase<T> : ISerialization
        where T : class
    {
        public abstract void Serialize(T source, FormatWriterBase writer);

        public abstract void Deserialize(T target, FormatReaderBase reader);

        void ISerialization.Serialize(object source, FormatWriterBase writer)
        {
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