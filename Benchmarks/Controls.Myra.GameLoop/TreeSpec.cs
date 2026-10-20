namespace Sachssoft.Engine.Controls.Benchmarks;
/// <summary>Provides identical counts and cell geometry to both UI libraries.</summary>
internal sealed class TreeSpec
{
    /// <summary>Initializes a new tree spec instance.</summary>
    public TreeSpec(int count)
    {
        Count = count;
        Columns = count switch
        {
            100 => 10,
            1000 => 40,
            5000 => 100,
            _ => throw new ArgumentOutOfRangeException(nameof(count))};
        Rows = count / Columns;
        CellWidth = 1200 / Columns;
        CellHeight = 800 / Rows;
    }

    /// <summary>Gets the number of buttons in the tree.</summary>
    public int Count { get; }
    /// <summary>Gets the number of equally sized columns.</summary>
    public int Columns { get; }
    /// <summary>Gets the number of equally sized rows.</summary>
    public int Rows { get; }
    /// <summary>Gets the width of each cell in logical pixels.</summary>
    public int CellWidth { get; }
    /// <summary>Gets the height of each cell in logical pixels.</summary>
    public int CellHeight { get; }
}
