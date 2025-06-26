namespace sachssoft.Sasogine.Document;

public interface IObjectFormatWriter<TWriter> : IObjectWriter<TWriter> where TWriter : FormatWriterBase
{
    void SaveTo(System.IO.Stream stream);
}
