namespace sachssoft.Sasogine.Document;

public delegate object? ObjectReaderGeneratorCallback(object reader);

public delegate IObjectReader<TReader>? ObjectReaderGeneratorCallback<TReader>(TReader reader) where TReader : FormatReaderBase;