using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using Avalonia.Threading;
using System;
using TetrisAvalonia.Models;
using TetrisAvalonia.ViewModels;

namespace TetrisAvalonia.Views;

public partial class MainWindow : Window
{
    private readonly TetrisGame _game;
    private readonly DispatcherTimer _gameTimer;
    private Canvas _gameCanvas;

    public MainWindow()
    {
        InitializeComponent();
        
        // Alternativ 1: Bruk this.Get<Canvas>("GameCanvas")
        _gameCanvas = this.Get<Canvas>("GameCanvas");
        
        // Alternativ 2: Hvis du foretrekker try-catch
        /*
        try {
            _gameCanvas = this.FindControl<Canvas>("GameCanvas") ?? 
                throw new InvalidOperationException("GameCanvas not found");
        }
        catch (Exception ex) {
            Console.WriteLine($"Error finding GameCanvas: {ex.Message}");
            throw;
        }
        */
        
        DataContext = new MainWindowViewModel();
        
        _game = new TetrisGame();
        _game.StartNewGame();

        _gameTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _gameTimer.Tick += GameTick;
        _gameTimer.Start();

        this.KeyDown += HandleInput;
    }

    private void GameTick(object? sender, EventArgs e)
    {
        if (sender == null) return;
        _game.UpdateGame(16);
        RenderGame();
    }

    private void HandleInput(object? sender, KeyEventArgs e)
    {
        if (_game == null || _game.IsGameOver || e == null) return;
        
        switch (e.Key)
        {
            case Key.Left:
                _game.MovePiece(-1, 0);
                break;
            case Key.Right:
                _game.MovePiece(1, 0);
                break;
            case Key.Up:
                _game.RotatePiece();
                break;
            case Key.Down:
                _game.MovePiece(0, 1);
                break;
            case Key.Space:
                _game.HardDrop();
                break;
            case Key.C:
                _game.HoldPiece();
                break;
        }
        RenderGame();
    }

    private void RenderGame()
    {
        _gameCanvas.Children.Clear();
        
        // Tegn grid
        for (int x = 0; x < TetrisGame.GridWidth; x++)
        {
            for (int y = 0; y < TetrisGame.GridHeight; y++)
            {
                if (_game.Grid[x, y] > 0)
                {
                    var block = new Rectangle
                    {
                        Width = 30,
                        Height = 30,
                        Fill = GetBrush(_game.Grid[x, y]),
                        Stroke = Brushes.Gray
                    };
                    Canvas.SetLeft(block, x * 30);
                    Canvas.SetTop(block, y * 30);
                    _gameCanvas.Children.Add(block);
                }
            }
        }
        
        // Tegn current piece
        if (_game.CurrentPiece != null)
        {
            for (int x = 0; x < _game.CurrentPiece.Width; x++)
            {
                for (int y = 0; y < _game.CurrentPiece.Height; y++)
                {
                    if (_game.CurrentPiece.Shape[x, y] > 0)
                    {
                        var block = new Rectangle
                        {
                            Width = 30,
                            Height = 30,
                            Fill = new SolidColorBrush(_game.CurrentPiece.Color),
                            Stroke = Brushes.White
                        };
                        Canvas.SetLeft(block, (_game.CurrentPosition.X + x) * 30);
                        Canvas.SetTop(block, (_game.CurrentPosition.Y + y) * 30);
                        _gameCanvas.Children.Add(block);
                    }
                }
            }
        }
    }

    private IBrush GetBrush(int value)
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
            _ => Brushes.Black
        };
    }
}
