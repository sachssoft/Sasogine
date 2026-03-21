
namespace Sachssoft.Sasogine.Editor;

public interface ITileStorage<TTile> where TTile : unmanaged
{

    void WriteTo(ref TTile blob);

    void ReadFrom(ref TTile blob);

}
