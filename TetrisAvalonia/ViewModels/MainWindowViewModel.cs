using Avalonia.Input;
using ReactiveUI;
using System.Windows.Input;
using TetrisAvalonia.Models;

namespace TetrisAvalonia.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private readonly TetrisGame _game;
    
    public ICommand RotateCommand { get; }
    public ICommand MoveLeftCommand { get; }
    public ICommand MoveRightCommand { get; }
    public ICommand DropCommand { get; }
    
    public MainWindowViewModel()
    {
        _game = new TetrisGame();
        _game.StartNewGame();
        
        RotateCommand = ReactiveCommand.Create(() => _game.RotatePiece());
        MoveLeftCommand = ReactiveCommand.Create(() => _game.MovePiece(-1, 0));
        MoveRightCommand = ReactiveCommand.Create(() => _game.MovePiece(1, 0));
        DropCommand = ReactiveCommand.Create(() => _game.HardDrop());
    }
}