using System.Collections.Generic;

namespace Sachssoft.Sasogine.World;

/// <summary>
/// Defines a container representing a single playable section of a game.
/// A stage can contain world nodes such as entities, tiles, objects,
/// or other elements belonging to that playable section.
/// </summary>
/// <remarks>
/// <para>
/// <c>Stage</c> is used as a neutral term that is independent of a
/// particular game genre.
/// </para>
/// <para>
/// Common genre-specific terms include:
/// </para>
/// <list type="bullet">
/// <item>
/// <description>
/// Platformer: often called a <c>Level</c>, representing a playable
/// section containing entities, tiles, and game logic.
/// </description>
/// </item>
/// <item>
/// <description>
/// Strategy or tactics: often called a <c>Scenario</c>, representing
/// a complete setup containing a map, units, and objectives.
/// </description>
/// </item>
/// <item>
/// <description>
/// RPG or adventure: often called a <c>Scene</c> or <c>Area</c>,
/// representing a location containing entities, events, and terrain.
/// </description>
/// </item>
/// </list>
/// <para>
/// Using the neutral term <c>Stage</c> allows the same world model
/// to be reused across different game genres.
/// </para>
/// </remarks>
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

    /// <summary>
    /// Gets the nodes contained in this playable stage.
    /// </summary>
    /// <remarks>
    /// Nodes represent the world elements that belong to the stage,
    /// such as entities or other objects participating in its world structure.
    /// </remarks>
    IEnumerable<IEntity> Nodes { get; }
}