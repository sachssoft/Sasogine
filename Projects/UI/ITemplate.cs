namespace Sachssoft.Engine.Controls;

/// <summary>Creates a presentation explicitly without reflection or runtime activation.</summary>
/// <typeparam name="TSource">The source supplied to the factory.</typeparam>
/// <typeparam name="TResult">The presentation produced by the factory.</typeparam>
public interface ITemplate<in TSource, out TResult>
{
    /// <summary>Creates a fresh presentation for the supplied source.</summary>
    /// <param name="source">The source being presented.</param>
    /// <returns>A new presentation owned by its consuming control.</returns>
    TResult Create(TSource source);
}
