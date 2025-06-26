namespace sachssoft.Sasogine.Providers;

public interface IGameDebugProvider
{
    void Write(string? message);

    void WriteLine(string? message);

    void Clear();
}
