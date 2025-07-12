namespace sachssoft.Sasogine.Services;

public interface IGameDebugService
{
    void Write(string? message);

    void WriteLine(string? message);

    void Clear();
}
