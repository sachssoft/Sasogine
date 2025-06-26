using System;

namespace sachssoft.Sasogine.Elements;

// Veraltetes Interface zur Rückwärtskompatibilität beibehalten
[Obsolete("Use IActiveGameObjectElement instead.")]
public interface IGameObjectElementActive
{
    void Update(GameContext context);
    void Draw(GameContext context);
}

// Neues Interface: erweitert das alte und das allgemeine IGameObjectElement
// => 100 % kompatibel, neue Architektur, alte API bleibt funktionsfähig
public interface IActiveGameObjectElement : IGameObjectElement, IGameObjectElementActive
{
    // keine zusätzlichen Member nötig – die Methoden werden geerbt
}


