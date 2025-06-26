namespace sachssoft.Sasogine.Document;

public interface IObjectReader<TReader> where TReader : FormatReaderBase
{
    void Read(TReader reader);
}