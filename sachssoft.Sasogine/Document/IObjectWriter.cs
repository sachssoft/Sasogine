using sachssoft.Sasogine.Document.Json;

namespace sachssoft.Sasogine.Document;

public interface IObjectWriter<TWriter> where TWriter : FormatWriterBase
{
    void Write(TWriter writer);
}
