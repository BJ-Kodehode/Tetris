using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace TetrisAvalonia.Models;

public class Tetromino
{
    public enum TetrominoType { I, O, T, S, Z, J, L }
    
    public TetrominoType Type { get; }
    public int[,] Shape { get; private set; }
    public Color Color { get; }
    public int Width => Shape.GetLength(0);
    public int Height => Shape.GetLength(1);
    
    private static readonly Dictionary<TetrominoType, int[,]> Shapes = new()
    {
        { TetrominoType.I, new int[4,4] { {0,0,0,0}, {1,1,1,1}, {0,0,0,0}, {0,0,0,0} } },
        { TetrominoType.O, new int[2,2] { {1,1}, {1,1} } },
        { TetrominoType.T, new int[3,3] { {0,1,0}, {1,1,1}, {0,0,0} } },
        { TetrominoType.S, new int[3,3] { {0,1,1}, {1,1,0}, {0,0,0} } },
        { TetrominoType.Z, new int[3,3] { {1,1,0}, {0,1,1}, {0,0,0} } },
        { TetrominoType.J, new int[3,3] { {1,0,0}, {1,1,1}, {0,0,0} } },
        { TetrominoType.L, new int[3,3] { {0,0,1}, {1,1,1}, {0,0,0} } }
    };
    
    private static readonly Dictionary<TetrominoType, Color> Colors = new()
    {
        { TetrominoType.I, Color.FromRgb(0, 255, 255) },
        { TetrominoType.O, Color.FromRgb(255, 255, 0) },
        { TetrominoType.T, Color.FromRgb(128, 0, 128) },
        { TetrominoType.S, Color.FromRgb(0, 255, 0) },
        { TetrominoType.Z, Color.FromRgb(255, 0, 0) },
        { TetrominoType.J, Color.FromRgb(0, 0, 255) },
        { TetrominoType.L, Color.FromRgb(255, 165, 0) }
    };
    
    public Tetromino(TetrominoType type)
    {
        Type = type;
        Shape = (int[,])Shapes[type].Clone();
        Color = Colors[type];
    }
    
    public void Rotate(bool clockwise = true)
    {
        var rotated = new int[Height, Width];
        
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (clockwise)
                    rotated[y, Width - 1 - x] = Shape[x, y];
                else
                    rotated[Height - 1 - y, x] = Shape[x, y];
            }
        }
        
        Shape = rotated;
    }
    
    public Tetromino Clone()
    {
        return new Tetromino(Type)
        {
            Shape = (int[,])Shape.Clone()
        };
    }
}