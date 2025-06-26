namespace sachssoft.Sasogine.Document;

public interface IObjectFormatReader<TReader> : IObjectReader<TReader> where TReader : FormatReaderBase
{
    void LoadFrom(System.IO.Stream stream);
}
