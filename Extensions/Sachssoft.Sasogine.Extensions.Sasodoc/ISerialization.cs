using Sachssoft.Sasodoc;

namespace Sachssoft.Sasogine.Extensions.Sasodoc
{
    public interface ISerialization
    {
        void Serialize(object source, FormatWriterBase writer);
        void Deserialize(object target, FormatReaderBase reader);
    }
}