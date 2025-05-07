using Avalonia.Media;

namespace TetrisAvalonia.Utilities;

public static class Constants
{
    // Define constants here
    public const string ExampleConstant = "Example";
    public const int CellSize = 30;
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