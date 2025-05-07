using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using TetrisAvalonia.Models;
using TetrisAvalonia.ViewModels;
using TetrisAvalonia.Utilities;

namespace TetrisAvalonia.Views;

public partial class MainWindow : Window
{
    private readonly TetrisGame _game;
    private readonly DispatcherTimer _gameTimer;
    private readonly Canvas _gameCanvas;
    private readonly List<Rectangle> _shadowBlocks = new();
    private readonly List<Rectangle> _pieceBlocks = new();

    public MainWindow()
    {
        InitializeComponent();
        
        // Initialiser spillbrettet
        _gameCanvas = this.FindControl<Canvas>("GameCanvas") ?? 
            throw new InvalidOperationException("GameCanvas not found");
        
        DataContext = new MainWindowViewModel();
        _game = new TetrisGame();
        _game.StartNewGame();
        _game.GameOver += OnGameOver;

        // Opprett blokker for skygge og brikke på forhånd
        InitializeBlockCache();

        _gameTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
        };
        _gameTimer.Tick += GameTick;
        _gameTimer.Start();

        this.KeyDown += HandleInput;
    }

    private void InitializeBlockCache()
    {
        // Forbered nok blokker for den største brikken (I-brikken: 4x4)
        for (int i = 0; i < 16; i++)
        {
            var shadowBlock = new Rectangle
            {
                Width = 30,
                Height = 30,
                Opacity = 0.3,
                IsVisible = false,
                Stroke = new SolidColorBrush(Colors.Gray) // Fiks for Pen til IBrush konvertering
            };
            _shadowBlocks.Add(shadowBlock);
            _gameCanvas.Children.Add(shadowBlock);

            var pieceBlock = new Rectangle
            {
                Width = 30,
                Height = 30,
                Stroke = new SolidColorBrush(Colors.White), // Fiks for Pen til IBrush konvertering
                IsVisible = false
            };
            _pieceBlocks.Add(pieceBlock);
            _gameCanvas.Children.Add(pieceBlock);
        }
    }

    private void GameTick(object? sender, EventArgs e)
    {
        if (IsGameActive())
        {
            _game?.UpdateGame(16); // Null-sjekk
            RenderGame();
        }
    }

    private bool IsGameActive()
    {
        return _game != null && !_game.IsGameOver;
    }

    private void HandleInput(object? sender, KeyEventArgs e)
    {
        if (!IsGameActive() || e == null) return;

        switch (e.Key)
        {
            case Key.Left:
                _game?.MovePiece(-1, 0); // Null-sjekk
                break;
            case Key.Right:
                _game?.MovePiece(1, 0); // Null-sjekk
                break;
            case Key.Up:
                _game?.RotatePiece(); // Null-sjekk
                break;
            case Key.Down:
                _game?.MovePiece(0, 1); // Null-sjekk
                break;
            case Key.Space:
                _game?.HardDrop(); // Null-sjekk
                break;
            case Key.C:
                _game?.HoldPiece(); // Null-sjekk
                break;
        }
        RenderGame();
    }

    private void RenderGame()
    {
        if (_gameCanvas == null || _game == null) return; // Null-sjekk

        // Skjul alle blokker først
        _shadowBlocks.ForEach(b => b.IsVisible = false);
        _pieceBlocks.ForEach(b => b.IsVisible = false);

        // Tegn grid
        for (int x = 0; x < TetrisGame.GridWidth; x++)
        {
            Debug.WriteLine($"Rendering column {x}");
            for (int y = 0; y < TetrisGame.GridHeight; y++)
            {
                Debug.WriteLine($"Rendering cell ({x}, {y})");
                if (_game.Grid[x, y] > 0)
                {
                    var block = new Rectangle
                    {
                        Width = 30,
                        Height = 30,
                        Fill = GetBrush(_game.Grid[x, y]),
                        Stroke = new SolidColorBrush(Colors.Gray) // Fiks for Pen til IBrush
                    };
                    Canvas.SetLeft(block, x * 30);
                    Canvas.SetTop(block, y * 30);
                    _gameCanvas.Children.Add(block);
                }
            }
        }

        if (_game.CurrentPiece != null)
        {
            // Tegn skygge
            RenderShadow();
            
            // Tegn aktiv brikke
            RenderCurrentPiece();
        }
    }

    private void RenderBlocks(IEnumerable<Rectangle> blocks, Point position, int[,] shape, Color color)
    {
        int blockIndex = 0;

        for (int x = 0; x < shape.GetLength(0); x++)
        {
            for (int y = 0; y < shape.GetLength(1); y++)
            {
                if (shape[x, y] > 0 && blockIndex < blocks.Count())
                {
                    var block = blocks.ElementAt(blockIndex);
                    block.Fill = new SolidColorBrush(color);
                    block.IsVisible = true;

                    Canvas.SetLeft(block, (position.X + x) * Constants.CellSize);
                    Canvas.SetTop(block, (position.Y + y) * Constants.CellSize);

                    blockIndex++;
                }
            }
        }
    }

    private void RenderShadow()
    {
        if (_game?.CurrentPiece == null) return;
        RenderBlocks(_shadowBlocks, _game.GetShadowPosition(), _game.CurrentPiece.Shape, _game.CurrentPiece.ShadowColor);
    }

    private void RenderCurrentPiece()
    {
        if (_game?.CurrentPiece == null) return;
        RenderBlocks(_pieceBlocks, _game.CurrentPosition, _game.CurrentPiece.Shape, _game.CurrentPiece.Color);
    }

    private static IBrush GetBrush(int value)
    {
        return BrushMapper.GetBrush(value);
    }

    private async void OnGameOver(object? sender, EventArgs e)
    {
        _gameTimer.Stop();

        // Prompt for player name
        var nameDialog = new Window
        {
            Title = "Enter Your Name",
            Width = 300,
            Height = 150
        };

        var nameTextBox = new TextBox { Width = 200, Margin = new Thickness(10) };
        var okButton = new Button
        {
            Content = "OK",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Command = ReactiveUI.ReactiveCommand.Create(() => Dispatcher.UIThread.InvokeAsync(() => nameDialog.Close()))
        };

        nameDialog.Content = new StackPanel
        {
            Children =
            {
                new TextBlock { Text = "Enter your name:", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Thickness(0, 20, 0, 10) },
                nameTextBox,
                okButton
            }
        };

        await nameDialog.ShowDialog(this);

        string playerName = nameTextBox.Text ?? "Anonymous";

        // Save highscore
        HighscoreManager.AddHighscore(playerName, _game.Score);

        // Show game over dialog
        var dialog = new Window
        {
            Title = "Game Over",
            Width = 300,
            Height = 150
        };

        dialog.Content = new StackPanel
        {
            Children =
            {
                new TextBlock { Text = "Game Over!", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Thickness(0, 20, 0, 20) },
                new Button
                {
                    Content = "OK",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Command = ReactiveUI.ReactiveCommand.Create(() => Dispatcher.UIThread.InvokeAsync(() => dialog.Close()))
                }
            }
        };

        await dialog.ShowDialog(this);
    }
}