namespace Sachssoft.Sasogine
{
    /// <summary>
    /// Defines a contract whose implementation is restricted to the
    /// declaring assembly.
    /// </summary>
    /// <remarks>
    /// Types outside the assembly can reference this interface but cannot
    /// provide a complete implementation because its contract contains
    /// internal members.
    /// </remarks>
    public interface IAssemblyContract
    {
        /// <summary>
        /// Initializes the implementing object.
        /// </summary>
        internal void Initialize();
    }
}