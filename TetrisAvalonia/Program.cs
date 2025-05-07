using Avalonia;
using Avalonia.Skia;
using System;

namespace TetrisAvalonia;

internal class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseSkia() // Explicitly use Skia as the rendering backend
            .WithInterFont()
            .LogToTrace();
}