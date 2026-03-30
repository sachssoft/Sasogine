namespace Sachssoft.Sasogine.Surface.Interactions
{
    /// <summary>
    /// Kontext einer einzelnen Request-Ausführung
    /// Enthält ID und Verweis auf den Request
    /// </summary>
    public sealed class RequestContext<TInput, TOutput>
    {
        public int Id { get; }
        public Request<TInput, TOutput> Request { get; }
        public TInput? Input { get; }

        internal RequestContext(int id, Request<TInput, TOutput> request, TInput? input = default)
        {
            Id = id;
            Request = request;
            Input = input;
        }

        /// <summary>
        /// Liefert das Ergebnis zurück an den Request
        /// </summary>
        public void SetResult(TOutput result)
        {
            Request.SetResult(Id, result);
        }

        public void SetResult()
        {
            Request.SetResult(Id, default!);
        }
    }
}