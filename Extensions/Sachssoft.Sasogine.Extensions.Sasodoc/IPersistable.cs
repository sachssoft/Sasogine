using Sachssoft.Sasodoc;

namespace Sachssoft.Sasogine.Extensions.Sasodoc
{
    public interface ISerialization
    {
        object Deserialize(FormatReaderBase reader);
        void Serialize(object source, FormatWriterBase writer);
    }
}