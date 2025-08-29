using System;

namespace Sachssoft.Sasogine.Elements;

// Neues Interface: erweitert das alte und das allgemeine IGameObjectElement
// => 100 % kompatibel, neue Architektur, alte API bleibt funktionsfähig
public interface IActiveGameObjectElement : IGameObjectElement
{
    // keine zusätzlichen Member nötig – die Methoden werden geerbt
    void Update(GameContext context);
    void Draw(GameContext context);
}


