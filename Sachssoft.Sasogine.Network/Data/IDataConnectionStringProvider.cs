namespace Sachssoft.Sasogine.Net;

public interface IDataConnectionStringProvider
{
    string? DatabaseName { get; }

    string? ToString();
}
