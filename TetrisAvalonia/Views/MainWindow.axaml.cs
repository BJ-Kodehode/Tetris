using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using System;
using TetrisAvalonia.Models;
using TetrisAvalonia.ViewModels;

namespace TetrisAvalonia.Views;

public partial class MainWindow : Window
{
    private readonly TetrisGame _game;
    private readonly DispatcherTimer _gameTimer;

    public MainWindow()
    {
        InitializeComponent();
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
        _game.UpdateGame(16);
        InvalidateVisual();
    }

    private void HandleInput(object? sender, KeyEventArgs e)
    {
        if (_game.IsGameOver) return;
        
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
        InvalidateVisual();
    }
}