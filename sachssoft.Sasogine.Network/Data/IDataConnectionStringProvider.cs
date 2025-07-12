namespace sachssoft.Sasogine.Net;

public interface IDataConnectionStringProvider
{
    string? DatabaseName { get; }

    string? ToString();
}
