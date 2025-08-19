namespace Sachssoft.Sasogine.Tiling;

public interface ITileMatrix
{
    int GetNeighborMask();
    int GetOrthogonalMask();
    int GetDiagonalMask();
    int MatchesLeft();
    int MatchesRight();
    int MatchesTop();
    int MatchesBottom();
    int MatchesTopLeft();
    int MatchesTopRight();
    int MatchesBottomLeft();
    int MatchesBottomRight();
}
