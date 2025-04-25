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
        { TetrominoType.I, Color.FromRgb(0, 255, 255) },    // Cyan
        { TetrominoType.O, Color.FromRgb(255, 255, 0) },     // Yellow
        { TetrominoType.T, Color.FromRgb(128, 0, 128) },     // Purple
        { TetrominoType.S, Color.FromRgb(0, 255, 0) },       // Green
        { TetrominoType.Z, Color.FromRgb(255, 0, 0) },       // Red
        { TetrominoType.J, Color.FromRgb(0, 0, 255) },        // Blue
        { TetrominoType.L, Color.FromRgb(255, 165, 0) }       // Orange
    };
    
    // Resten av koden...
}