namespace Sachssoft.Sasogine.Net;

public class LocalHostConnectionStringProvider : IDataConnectionStringProvider
{
    public string? Server { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? DatabaseName { get; set; }

    public override string ToString()
    {
        return $"Server={Server};User ID={Username};Password={Password};Database={DatabaseName}";
    }
}
