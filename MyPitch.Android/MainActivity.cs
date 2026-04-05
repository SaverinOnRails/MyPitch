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
    Label = "MyPitch",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        var path = System.IO.Path.Combine(FilesDir!.AbsolutePath, "default.sf2");
        var path2 = System.IO.Path.Combine(FilesDir!.AbsolutePath, "warm pad.sf2");
        if (!File.Exists(path))
        {
            var assets = Assets;
            using var stream = assets!.Open("default.sf2");
            using var fileStream = File.Create(path);
            stream.CopyTo(fileStream);
        }
        if (!File.Exists(path2))
        {
            var assets = Assets;
            using var stream = assets!.Open("warm pad.sf2");
            using var fileStream = File.Create(path2);
            stream.CopyTo(fileStream);
        }
        PlatformServiceProvider.AudioDriver = new AudioTrackDriver(path,path2);
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }

}
