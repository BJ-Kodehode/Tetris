using Avalonia.Media;

namespace TetrisAvalonia.Utilities;

public static class Constants
{
    public const int GridWidth = 10;
    public const int GridHeight = 20;
    public const int CellSize = 30;
    public const int InitialFallSpeed = 500; // ms
}

public static class BrushMapper
{
    public static IBrush GetBrush(int value)
    {
        return value switch
        {
            1 => Brushes.Cyan,
            2 => Brushes.Yellow,
            3 => Brushes.Purple,
            4 => Brushes.Green,
            5 => Brushes.Red,
            6 => Brushes.Blue,
            7 => Brushes.Orange,
            _ => Brushes.Transparent
        };
    }
}