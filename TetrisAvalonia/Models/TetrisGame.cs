using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TetrisAvalonia.Utilities;

namespace TetrisAvalonia.Models;

public class TetrisGame
{
    public event EventHandler? GameOver;
    // The event 'LinesCleared' is declared but not used. Consider removing it if unnecessary.
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

    private const double MinFallInterval = 50;
    private static readonly int[] WallKickOffsets = { 0, -1, 1, -2, 2 };

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
        
        for (int i = 0; i < WallKickOffsets.Length; i++)
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
        if (CurrentPiece == null) return;

        int dropDistance = 0;
        while (IsValidPosition(CurrentPiece.Shape, (int)CurrentPosition.X, (int)CurrentPosition.Y + dropDistance + 1))
        {
            dropDistance++;
        }

        CurrentPosition = new Point(CurrentPosition.X, CurrentPosition.Y + dropDistance);
        LockPiece();
        SpawnNewPiece();
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
    
    private Tetromino GetRandomPiece()
    {
        var values = Enum.GetValues(typeof(Tetromino.TetrominoType));
        var randomValue = values.GetValue(_random.Next(values.Length));
        if (randomValue == null)
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
                
                if (boardX < 0 || boardX >= GridWidth || boardY >= GridHeight || boardY < 0)
                    return false;
                    
                if (boardY >= 0 && Grid[boardX, boardY] != 0)
                    return false;
            }
        }
        return true;
    }
    
    private void LockPiece()
    {
        if (CurrentPiece == null || NextPiece == null) return;

        Console.WriteLine("Locking piece at position: " + CurrentPosition);
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

        Console.WriteLine("Grid after locking piece:");
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                Console.Write(Grid[x, y] + " ");
            }
            Console.WriteLine();
        }

        OptimizeClearLines();
        UpdateScore(TotalLines);

        if (!IsValidPosition(NextPiece.Shape, GridWidth / 2 - NextPiece.Width / 2, 0))
        {
            IsGameOver = true;
            Console.WriteLine("Game over! Final score: " + Score);
            HighscoreManager.AddHighscore("Player", Score); // Oppdaterer highscores
            GameOver?.Invoke(this, EventArgs.Empty);
        }
    }
    
    private void OptimizeClearLines()
    {
        int targetRow = GridHeight - 1;

        Console.WriteLine("Optimizing clear lines...");
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

            if (!lineComplete)
            {
                if (targetRow != y)
                {
                    for (int x = 0; x < GridWidth; x++)
                    {
                        Grid[x, targetRow] = Grid[x, y];
                    }
                }
                targetRow--;
            }
        }

        // Clear remaining rows above targetRow
        for (int y = targetRow; y >= 0; y--)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                Grid[x, y] = 0;
            }
        }

        int linesCleared = GridHeight - 1 - targetRow;
        Console.WriteLine($"Lines cleared: {linesCleared}");
        if (linesCleared > 0)
        {
            LinesCleared?.Invoke(this, linesCleared);
        }

        Console.WriteLine("Grid after clearing lines:");
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                Console.Write(Grid[x, y] + " ");
            }
            Console.WriteLine();
        }
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
        return Math.Max(MinFallInterval, 1000 - (Level - 1) * 50);
    }
    
    private void SpawnNewPiece()
    {
        if (NextPiece == null) return;

        Console.WriteLine("Spawning new piece...");
        CurrentPiece = NextPiece;
        NextPiece = GetRandomPiece();
        CurrentPosition = new Point(GridWidth / 2 - CurrentPiece.Width / 2, 0);
        CanHold = true;

        Console.WriteLine("New piece spawned: " + CurrentPiece.Type);
        Console.WriteLine("Current position: " + CurrentPosition);
        Console.WriteLine("Grid after spawning new piece:");
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                Console.Write(Grid[x, y] + " ");
            }
            Console.WriteLine();
        }
    }

    public Point GetShadowPosition()
    {
        if (CurrentPiece == null) return CurrentPosition;

        int low = (int)CurrentPosition.Y;
        int high = GridHeight;

        while (low < high)
        {
            Debug.WriteLine($"While loop: low={low}, high={high}");
            int mid = (low + high) / 2;
            if (IsValidPosition(CurrentPiece.Shape, (int)CurrentPosition.X, mid))
            {
                low = mid + 1;
            }
            else
            {
                high = mid;
            }
        }

        return new Point(CurrentPosition.X, low - 1);
    }
}