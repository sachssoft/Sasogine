using System;

namespace Sachssoft.Sasogine.Tiling
{
    // Delegate für das Event
    public delegate void NotifyMapChangedEventHandler(object? sender, NotifyMapChangedEventArgs args);

    public class NotifyMapChangedEventArgs : EventArgs
    {
        // Alte und neue Tiles
        public IOutputTile? OldTile { get; }
        public IOutputTile? NewTile { get; }

        // Alte und neue Indizes
        public int Index { get; }

        // Leerer Konstruktor
        public NotifyMapChangedEventArgs(int index)
        {
            Index = index;
        }

        // Nur neues Tile
        public NotifyMapChangedEventArgs(int index, IOutputTile? newTile)
        {
            NewTile = newTile;
            Index = index;
        }

        // Altes und neues Tile
        public NotifyMapChangedEventArgs(int index, IOutputTile? oldTile, IOutputTile? newTile)
        {
            OldTile = oldTile;
            NewTile = newTile;
            Index = index;
        }
    }
}
