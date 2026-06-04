using Sachssoft.Sasodoc;

namespace Sachssoft.Sasogine.Extensions.Sasodoc
{
    public static class PersistableExtensions
    {
        public static ISerialization? ReadPersistable(this FormatReaderBase reader, string property, PersistableRegistry registry, ISerialization? fallback, FormatOptions? options = null)
        {
            if (reader.Contains("_type"))
            {
                var persistableReader = reader.Read(property);

                if (persistableReader != null)
                {
                    persistableReader.Options = options;
                    var persistableName = persistableReader.ReadString(registry.TypePropertyName);
                    if (registry.TryCreate(persistableName, out var result))
                    {
                        result.Deserialize(persistableReader);
                        return result;
                    }
                }
            }

            return fallback;
        }

        public static ISerialization[]? ReadPersistableArray(this FormatReaderBase reader, string property, PersistableRegistry registry, ISerialization[]? fallback, FormatOptions? options = null)
        {
            if (reader.Contains(property))
            {
                var persistables = new List<ISerialization>();
                var readerList = new List<FormatReaderBase>(reader.ReadArray(property));
                foreach (var persistableReader in readerList)
                {
                    persistableReader.Options = options;
                    var persistableName = persistableReader.ReadString(registry.TypePropertyName);
                    if (registry.TryCreate(persistableName, out var result))
                    {
                        result.Deserialize(persistableReader);
                        persistables.Add(result);
                    }
                }
                return persistables.ToArray();
            }

            return fallback;
        }


        //public static void WritePersistable(this FormatWriterBase writer, string property, PersistableRegistry registry, ISerialization? persistableonent, FormatOptions? options = null)
        //{
        //    if (persistableonent == null)
        //        return;

        //    var persistableWriter = writer.CreateWriter();
        //    persistableWriter.Options = options;
        //    persistableWriter.WriteString(registry.TypePropertyName, registry.FindName(persistableonent.GetType()));
        //    persistableonent.Serialize(persistableWriter);
        //    writer.Write(property, persistableWriter);
        //}

        //public static void WritePersistableArray(this FormatWriterBase writer, string property, PersistableRegistry registry, ISerialization[]? persistableonents, FormatOptions? options = null)
        //{
        //    if (persistableonents == null)
        //        return;

        //    var writerList = new List<FormatWriterBase>();
        //    foreach (var persistableonent in persistableonents)
        //    {
        //        var persistableWriter = writer.CreateWriter();
        //        persistableWriter.Options = options;
        //        persistableWriter.WriteString(registry.TypePropertyName, registry.FindName(persistableonent.GetType()));
        //        persistableonent.Serialize(persistableWriter);
        //        writerList.Add(persistableWriter);
        //    }
        //    writer.WriteArray(property, writerList.ToArray());
        //}

        //public static void TraverseReference(this FormatReaderBase reader, string property, ISerialization persistable, FormatOptions? options = null)
        //{
        //    if (persistable == null)
        //        throw new ArgumentNullException(nameof(persistable));

        //    if (reader.Contains(property))
        //    {
        //        var persistableReader = reader.Read(property);

        //        if (persistableReader != null)
        //        {
        //            persistableReader.Options = options;
        //            persistable.Deserialize(persistableReader);
        //        }
        //    }
        //}

        //public static void EmitReference(this FormatWriterBase writer, string property, ISerialization persistable, FormatOptions? options = null)
        //{
        //    if (persistable == null)
        //        throw new ArgumentNullException(nameof(persistable));

        //    var persistableWriter = writer.CreateWriter();
        //    persistableWriter.Options = options;
        //    persistable.Serialize(persistableWriter);
        //    writer.Write(property, persistableWriter);
        //}
    }
}
