using Avalonia;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MyPitch.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break
    [STAThread]
    public static void Main(string[] args)
    {
        NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
        PlatformServiceProvider.AudioDriver = new FluidAudioDriver(Path.Join(AppContext.BaseDirectory, "default.sf2"));
        BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
        if (PlatformServiceProvider.AudioDriver is IDisposable d) d.Dispose();
    }

    private static nint DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName == "fluidsynth")
        {
            if (OperatingSystem.IsWindows())
            {
                return NativeLibrary.Load("libfluidsynth-3.dll");

            }
            else if (OperatingSystem.IsLinux())
            {
                return NativeLibrary.Load("libfluidsynth.so.3");

            }
            else return nint.Zero;
        }

        // Otherwise, fallback to default import resolver.
        return nint.Zero;
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
