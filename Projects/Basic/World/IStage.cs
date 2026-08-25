using System.Collections.Generic;

namespace Sachssoft.Sasogine.World
{
    /// <summary>
    /// Stage is a neutral name for a container representing a single playable section of a game.
    /// Suitable for platformers, strategy, RPGs, or other genres.
    /// 
    /// Game-type conventions:
    /// - Jump-'n'-Run / Platformer: often called "Level" – a single segment with entities, tiles, and logic.
    /// - Strategy / Tactics: often called "Scenario" – the complete setup including map, units, and objectives.
    /// - RPG / Adventure: often called "Scene" or "Area" – a location or area containing entities, events, and terrain.
    /// 
    /// Stage was chosen as a neutral, reusable name suitable for multiple genres, 
    /// so the same library can handle platformers, strategy, RPGs, etc.
    /// </summary>
    // Stage ist ein neutraler Name für einen Container, der einen einzelnen spielbaren Abschnitt im Spiel repräsentiert.
    // Geeignet für Plattformspiele, Strategie-, RPG- oder andere Genres.
    //
    // Konventionen je nach Spieltyp:
    // - Jump-'n'-Run / Plattformspiel: wird oft „Level“ genannt – ein einzelner Abschnitt mit Entities, Tiles und Logik.
    // - Strategie / Taktik: wird oft „Scenario“ genannt – das komplette Setup inklusive Karte, Einheiten und Zielen.
    // - RPG / Adventure: wird oft „Scene“ oder „Area“ genannt – ein Ort oder Bereich mit Entities, Events und Gelände.
    //
    // Stage wurde als neutraler, wiederverwendbarer Name gewählt, 
    // sodass dieselbe Library Plattformspiele, Strategie-, RPG-Spiele usw. unterstützen kann.
    public interface IStage
    {
        // Hier kommen später die Properties und Methoden wie Entities, Tiles und Scripts

        IEnumerable<IEntity> Nodes { get; }
    }
}