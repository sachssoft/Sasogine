using Sachssoft.Sasogine.Surface.Interactions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Surface.Interactions
{
    /// <summary>
    /// Repräsentiert eine einfache Anforderung (Request) von einem ViewModel oder 
    /// logischen System an die Oberfläche (Surface/UI), die ein Ergebnis zurückliefern soll.
    /// 
    /// Diese Klasse ist funktional vergleichbar mit "Interaction<TInput, TOutput>" aus ReactiveUI,
    /// jedoch vollständig eigenständig, leichtgewichtig und ohne externe Abhängigkeiten.
    /// 
    /// Verwendung:
    /// - Ein System (z. B. ViewModel) sendet über SendAsync(...) eine Anforderung
    ///   an die UI (z. B. Dialog, Auswahl, Bestätigung).
    /// - Die UI registriert einen Handler und liefert das Ergebnis später asynchron
    ///   über SetResult(...) zurück.
    /// 
    /// Zweck:
    /// - Entkopplung zwischen Logik und Oberfläche
    /// - Ermöglicht UI-Operationen, die ein Ergebnis benötigen (z. B. ConfirmDialog)
    /// - Ideal für Tools, Editoren oder UI-Prompts in Sasogine.Surface
    /// 
    /// Hinweis:
    /// - Nur eine Klasse, bewusst minimal, performant und AOT-freundlich.
    /// </summary>

    /// <summary>
    /// Allgemeiner Request für Sasogine.Surface
    /// - Input und Output als object
    /// - Kann Input + Output, nur Input oder nur Output verwenden
    /// - Async/await-fähig für Output
    /// </summary>
    /// 

    // TInput und TOutput haben Standardwert object

    /// <summary>
    /// Allgemeiner Request für Sasogine.Surface
    /// - Input: TInput (oder object, wenn nicht benötigt)
    /// - Output: TOutput (oder object, wenn nicht benötigt)
    /// - Async/await für Output möglich
    /// </summary>
    /// <summary>
    /// Minimalistische Request-Klasse für Input + Output
    /// Async/await-fähig, eine zentrale Handler-Variante
    /// </summary>
    public sealed class Request<TInput, TOutput>
    {
        // Handler: bekommt RequestContext, optional Input
        public delegate void Handler(RequestContext<TInput, TOutput> ctx);

        private Handler? _handler;
        private int _idCounter;
        private readonly Dictionary<int, TaskCompletionSource<TOutput>> _pending = new(4);

        // Konstruktor privat → Factory
        private Request() { }

        /// <summary>
        /// Factory-Methode
        /// </summary>
        public static Request<TInput, TOutput> Create(Handler? handler = null)
        {
            return new Request<TInput, TOutput> { _handler = handler };
        }

        /// <summary>
        /// Sendet einen Request
        /// </summary>
        public Task<TOutput> SendAsync(TInput input = default!)
        {
            if (_handler == null)
                throw new InvalidOperationException("Handler nicht gesetzt.");

            int id = ++_idCounter;
            var tcs = new TaskCompletionSource<TOutput>();
            _pending.Add(id, tcs);

            var ctx = new RequestContext<TInput, TOutput>(id, this, input);
            _handler(ctx);

            return tcs.Task;
        }

        public Task<TOutput> SendAsync() => SendAsync(default!);

        /// <summary>
        /// Liefert das Ergebnis zurück
        /// </summary>
        internal void SetResult(int id, TOutput result)
        {
            if (_pending.TryGetValue(id, out var tcs))
            {
                _pending.Remove(id);
                tcs.TrySetResult(result);
            }
        }
    }
}

// Beispiele

//// Input + Output
//var request = Request<string, bool>.Create(ctx =>
//{
//    Console.WriteLine("Frage: " + ctx.Input);
//    // Simuliertes Dialog-Ergebnis
//    ctx.SetResult(true);
//});

//bool result = await request.SendAsync("Willst du löschen?");
//Console.WriteLine(result);

//// Nur Output
//var requestOut = Request<object?, int>.Create(ctx =>
//{
//    int val = new Random().Next(100);
//    ctx.SetResult(val);
//});

//int output = await requestOut.SendAsync();
//Console.WriteLine(output);

//// Nur Input
//var requestIn = Request<string, object?>.Create(ctx =>
//{
//    Console.WriteLine("Nur Input: " + ctx.Input);
//    ctx.SetResult(null); // Output ignorieren
//});

//await requestIn.SendAsync("Test Input");
