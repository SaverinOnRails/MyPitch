using System;
using System.IO;
using Avalonia;

namespace MyPitch.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        ServiceProvider.AudioDriver = new OpenAlAudioDriver(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "default.sf2"));
        BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
        if (ServiceProvider.AudioDriver is IDisposable d) d.Dispose();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
