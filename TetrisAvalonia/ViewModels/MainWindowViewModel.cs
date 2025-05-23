using Avalonia.Input;
using ReactiveUI;
using System.Windows.Input;
using TetrisAvalonia.Models;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia;
using System.Collections.ObjectModel;
using TetrisAvalonia.Utilities;
using Avalonia.Threading;

namespace TetrisAvalonia.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private readonly TetrisGame _game;
    private readonly Canvas _gameCanvas = new Canvas();
    private int _score;

    public ICommand RotateCommand { get; }
    public ICommand MoveLeftCommand { get; }
    public ICommand MoveRightCommand { get; }
    public ICommand DropCommand { get; }
    public ObservableCollection<Highscore> Highscores { get; }

    public int Score
    {
        get => _score;
        set => this.RaiseAndSetIfChanged(ref _score, value);
    }

    public MainWindowViewModel()
    {
        _game = new TetrisGame();
        _game.StartNewGame();

        Highscores = new ObservableCollection<Highscore>(HighscoreManager.LoadHighscores());

        RotateCommand = ReactiveCommand.Create(() => Dispatcher.UIThread.Post(() => _game.RotatePiece()));
        MoveLeftCommand = ReactiveCommand.Create(() => Dispatcher.UIThread.Post(() => _game.MovePiece(-1, 0)));
        MoveRightCommand = ReactiveCommand.Create(() => Dispatcher.UIThread.Post(() => _game.MovePiece(1, 0)));
        DropCommand = ReactiveCommand.Create(() => Dispatcher.UIThread.Post(() => _game.HardDrop()));

        _game.ScoreUpdated += (sender, newScore) =>
        {
            Score = newScore;
        };
    }

    private SolidColorBrush GetBrush(int value)
    {
        return (SolidColorBrush)BrushMapper.GetBrush(value);
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

        // Tegn skygge først (så den ligger under den aktive brikken)
        if (_game.CurrentPiece != null)
        {
            Point shadowPosition = _game.GetShadowPosition();
            for (int x = 0; x < _game.CurrentPiece.Width; x++)
            {
                for (int y = 0; y < _game.CurrentPiece.Height; y++)
                {
                    if (_game.CurrentPiece.Shape[x, y] > 0)
                    {
                        var shadow = new Rectangle
                        {
                            Width = 30,
                            Height = 30,
                            Fill = new SolidColorBrush(Colors.Transparent),
                            Stroke = Brushes.Gray,
                            Opacity = 0.5
                        };
                        Canvas.SetLeft(shadow, (shadowPosition.X + x) * 30);
                        Canvas.SetTop(shadow, (shadowPosition.Y + y) * 30);
                        _gameCanvas.Children.Add(shadow);
                    }
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
}