using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace TetrisAvalonia.Models;

public class TetrisGame
{
    public const int GridWidth = 10;
    public const int GridHeight = 20;
    public int[,] Grid { get; private set; } = new int[GridWidth, GridHeight];
    
    // Resten av koden forblir den samme...
    
    public void StartNewGame()
    {
        Grid = new int[GridWidth, GridHeight];
        Score = 0;
        Level = 1;
        TotalLines = 0;
        IsGameOver = false;
        _fallInterval = CalculateFallInterval();
        InitializePieces();
    }
    
    // Resten av metodene...
}