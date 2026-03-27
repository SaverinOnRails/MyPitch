using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Avalonia;
using Avalonia.Android;
using Java.Nio.FileNio;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MyPitch.Droid;

[Activity(
    Label = "MyPitch.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        var path = System.IO.Path.Combine(FilesDir!.AbsolutePath, "default.sf2");

        if (!File.Exists(path))
        {
            var assets = Assets;
            using var stream = assets!.Open("default.sf2");
            using var fileStream = File.Create(path);
            stream.CopyTo(fileStream);
        }
        ServiceProvider.AudioDriver = new AudioTrackDriver(path);
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
  
}
