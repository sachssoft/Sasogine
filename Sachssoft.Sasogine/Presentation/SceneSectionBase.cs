using System;

namespace Sachssoft.Sasogine.Presentation
{
    /* 
       Basisklasse für Szenenabschnitte (Sections) innerhalb der Präsentation.

       Jede Section gehört genau zu einer Scene (_owner) und kapselt einen Teil der
       Logik oder des Zustands. Dies ist optional und eher für fortgeschrittene Szenen
       mit sehr viel Code gedacht, um Übersichtlichkeit zu schaffen.

       Durch die Aufteilung in Sections kann eine große Scene modularisiert werden,
       was Wartbarkeit und Lesbarkeit deutlich verbessert.

       Die Klasse erbt von NotifyObject, wodurch PropertyChanged-Benachrichtigungen
       möglich sind – typisch für MVVM-ähnliche ViewModel-Logik.

       IDisposable wird bereitgestellt, damit Sections bei Bedarf Ressourcen freigeben
       oder Events abmelden können. Die Basisimplementierung tut nichts; konkrete
       Sections überschreiben Dispose bei Bedarf.
    */

    //public abstract class SceneSectionBase : NotifyObject, IDisposable
    public abstract class SceneSectionBase : IDisposable
    {
        private readonly SceneBase _owner;

        public SceneSectionBase(SceneBase owner)
        {
            _owner = owner;
        }

        protected SceneBase Owner => _owner;

        public virtual void Dispose()
        {
            // Basisklasse macht nichts; abgeleitete Sections können hier Cleanup-Logik implementieren.
        }
    }
}
