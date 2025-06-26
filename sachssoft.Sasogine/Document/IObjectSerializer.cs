namespace sachssoft.Sasogine.Document;

public interface IObjectSerializer<TReader, TWriter>
     where TReader : FormatReaderBase
     where TWriter : FormatWriterBase
{
}
