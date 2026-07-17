namespace Sachssoft.Sasogine
{
    // Markiert einen Vertrag, der nur von der eigenen Assembly
    // implementiert werden darf.
    // Externe Implementierungen sind nicht möglich. 
    public interface IAssemblyContract
    {
        internal void Initialize();
    }
}
