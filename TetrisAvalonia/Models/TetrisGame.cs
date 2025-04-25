using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace TetrisAvalonia.Models;

public class TetrisGame
{
    public event EventHandler? GameOver;
    public event EventHandler<int>? LinesCleared;
    public event EventHandler<int>? ScoreUpdated;
    
    public const int GridWidth = 10;
    public const int GridHeight = 20;
    public int[,] Grid { get; private set; }
    public Tetromino? CurrentPiece { get; private set; }
    public Point CurrentPosition { get; private set; }
    public Tetromino? NextPiece { get; private set; }
    public Tetromino? HeldPiece { get; private set; }
    public bool CanHold { get; private set; }
    
    public int Score { get; private set; }
    public int Level { get; private set; }
    public int TotalLines { get; private set; }
    public bool IsGameOver { get; private set; }
    
    private readonly Random _random = new();
    private double _fallInterval;
    private double _accumulatedTime;
    private bool _isHardDropping;

    private const double MinFallInterval = 50; // Fix: Extracted magic number as a constant
    private static readonly int[] WallKickOffsets = { 0, -1, 1, -2, 2 }; // Fix: Improved wall kick offsets

    public TetrisGame()
    {
        Grid = new int[GridWidth, GridHeight];
        StartNewGame();
    }
    
    public void StartNewGame()
    {
        Array.Clear(Grid, 0, Grid.Length);
        Score = 0;
        Level = 1;
        TotalLines = 0;
        IsGameOver = false;
        _fallInterval = CalculateFallInterval();
        InitializePieces();
        ScoreUpdated?.Invoke(this, Score);
    }
    
    private void InitializePieces()
    {
        CurrentPiece = GetRandomPiece();
        NextPiece = GetRandomPiece();
        HeldPiece = null;
        CurrentPosition = new Point(GridWidth / 2 - (CurrentPiece?.Width ?? 0) / 2, 0);
        CanHold = true;
    }
    
    public void UpdateGame(double deltaTime)
    {
        if (IsGameOver || CurrentPiece == null) return;
        
        _accumulatedTime += deltaTime;
        
        if (_accumulatedTime >= _fallInterval || _isHardDropping)
        {
            _accumulatedTime = 0;
            if (!MovePiece(0, 1))
            {
                LockPiece();
                SpawnNewPiece();
            }
            _isHardDropping = false;
        }
    }
    
    public bool MovePiece(int deltaX, int deltaY)
    {
        if (CurrentPiece == null) return false;
        
        var newPosition = new Point(CurrentPosition.X + deltaX, CurrentPosition.Y + deltaY);
        
        if (IsValidPosition(CurrentPiece.Shape, (int)newPosition.X, (int)newPosition.Y))
        {
            CurrentPosition = newPosition;
            return true;
        }
        return false;
    }
    
    public void RotatePiece(bool clockwise = true)
    {
        if (CurrentPiece == null) return;
        
        var rotatedPiece = CurrentPiece.Clone();
        rotatedPiece.Rotate(clockwise);
        
        for (int i = 0; i < WallKickOffsets.Length; i++) // Fix: Use WallKickOffsets array
        {
            int offsetX = WallKickOffsets[i];
            if (IsValidPosition(rotatedPiece.Shape, (int)(CurrentPosition.X + offsetX), (int)CurrentPosition.Y))
            {
                CurrentPiece = rotatedPiece;
                CurrentPosition = new Point(CurrentPosition.X + offsetX, CurrentPosition.Y);
                return;
            }
        }
    }
    
    public void HardDrop()
    {
        _isHardDropping = true;
        while (MovePiece(0, 1)) { }
    }
    
    public void HoldPiece()
    {
        if (!CanHold || CurrentPiece == null) return;
        
        var temp = HeldPiece;
        HeldPiece = CurrentPiece;
        
        if (temp != null)
        {
            CurrentPiece = temp;
            CurrentPosition = new Point(GridWidth / 2 - CurrentPiece.Width / 2, 0);
        }
        else
        {
            SpawnNewPiece();
        }
        
        CanHold = false;
    }
    
    private int GetWallKickOffset(int attempt, int pieceWidth)
    {
        return attempt >= 0 && attempt < WallKickOffsets.Length ? WallKickOffsets[attempt] : 0;
    }
    
    private Tetromino GetRandomPiece()
    {
        var values = Enum.GetValues(typeof(Tetromino.TetrominoType));
        var randomValue = values.GetValue(_random.Next(values.Length));
        if (randomValue == null) // Fix: Null check for unboxing
        {
            throw new InvalidOperationException("Failed to retrieve a valid TetrominoType.");
        }
        return new Tetromino((Tetromino.TetrominoType)randomValue);
    }
    
    private bool IsValidPosition(int[,] shape, int x, int y)
    {
        for (int py = 0; py < shape.GetLength(1); py++)
        {
            for (int px = 0; px < shape.GetLength(0); px++)
            {
                if (shape[px, py] == 0) continue;
                
                int boardX = x + px;
                int boardY = y + py;
                
                if (boardX < 0 || boardX >= GridWidth || boardY >= GridHeight || boardY < 0) // Fix: Added check for boardY < 0
                    return false;
                    
                if (boardY >= 0 && Grid[boardX, boardY] != 0)
                    return false;
            }
        }
        return true;
    }
    
    private void LockPiece()
    {
        if (CurrentPiece == null || NextPiece == null) return; // Fix: Null check for CurrentPiece and NextPiece
        
        for (int y = 0; y < CurrentPiece.Height; y++)
        {
            for (int x = 0; x < CurrentPiece.Width; x++)
            {
                if (CurrentPiece.Shape[x, y] != 0)
                {
                    int boardX = (int)CurrentPosition.X + x;
                    int boardY = (int)CurrentPosition.Y + y;
                    
                    if (boardY >= 0)
                    {
                        Grid[boardX, boardY] = (int)CurrentPiece.Type + 1;
                    }
                }
            }
        }
        
        int lines = ClearLines();
        UpdateScore(lines);
        
        if (!IsValidPosition(NextPiece.Shape, GridWidth / 2 - NextPiece.Width / 2, 0))
        {
            IsGameOver = true;
            GameOver?.Invoke(this, EventArgs.Empty);
        }
    }
    
    private int ClearLines()
    {
        int linesCleared = 0;
        
        for (int y = GridHeight - 1; y >= 0; y--)
        {
            bool lineComplete = true;
            
            for (int x = 0; x < GridWidth; x++)
            {
                if (Grid[x, y] == 0)
                {
                    lineComplete = false;
                    break;
                }
            }
            
            if (lineComplete)
            {
                linesCleared++;
                
                for (int ny = y; ny > 0; ny--)
                {
                    for (int x = 0; x < GridWidth; x++)
                    {
                        Grid[x, ny] = Grid[x, ny - 1];
                    }
                }
                
                for (int x = 0; x < GridWidth; x++)
                {
                    Grid[x, 0] = 0;
                }
                
                y++;
            }
        }
        
        if (linesCleared > 0)
        {
            LinesCleared?.Invoke(this, linesCleared);
        }
        
        return linesCleared;
    }
    
    private void UpdateScore(int linesCleared)
    {
        if (linesCleared == 0) return;
        
        TotalLines += linesCleared;
        Level = TotalLines / 10 + 1;
        _fallInterval = CalculateFallInterval();
        
        int points = linesCleared switch
        {
            1 => 100 * Level,
            2 => 300 * Level,
            3 => 500 * Level,
            4 => 800 * Level,
            _ => 0
        };
        
        Score += points;
        ScoreUpdated?.Invoke(this, Score);
    }
    
    private double CalculateFallInterval()
    {
        return Math.Max(50, 1000 - (Level - 1) * 50);
    }
    
    private void SpawnNewPiece()
    {
        if (NextPiece == null) return;
        
        CurrentPiece = NextPiece;
        NextPiece = GetRandomPiece();
        CurrentPosition = new Point(GridWidth / 2 - CurrentPiece.Width / 2, 0);
        CanHold = true;
    }
}