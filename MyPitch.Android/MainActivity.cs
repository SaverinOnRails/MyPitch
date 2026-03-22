using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using SilkAudioDriver;

namespace MyPitch.Android;

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
        ServiceProvider.AudioDriver = new OpenAlAudioDriver();
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}
