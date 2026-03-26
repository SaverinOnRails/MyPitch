using Avalonia;
using Avalonia.Browser;
using MyPitch;
using MyPitch.Browser;
using System;
using System.Net.Http;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading.Tasks;
namespace MyPitch.Browser;
internal sealed partial class Program
{
    private static async Task Main(string[] args)
    {
        await JSHost.ImportAsync("Interop", "../js/WebAudio/WebAudio.js");
        var http = new HttpClient
        {
            BaseAddress = new Uri(Interop.BaseHref())
        };
        ServiceProvider.AudioDriver = await WebAudioDriver.CreateAsync(http);
        await BuildAvaloniaApp()
            .WithInterFont()
            .StartBrowserAppAsync("out");
    }
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}

internal static partial class Interop
{
    [JSImport("baseHref","Interop")]
    public static partial string BaseHref();

    [JSImport("startAudioServer", "Interop")]
    public static partial void StartAudioServer();

    [JSImport("postAudioData", "Interop")]
    public static partial void PostAudio(
        nint buffer, int length
    );

    [JSExport]
    public static nint WriteToSink()
    {
        return ((WebAudioDriver)ServiceProvider.AudioDriver).WriteToSink();
    }
}

