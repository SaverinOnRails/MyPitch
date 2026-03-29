using Avalonia;
using Avalonia.Browser;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
namespace MyPitch.Browser;

internal sealed partial class Program
{
    private static async Task Main(string[] args)
    {
        await JSHost.ImportAsync("Interop", "../js/synth.js");
        ServiceProvider.AudioDriver = new WebAudioDriver();
        await BuildAvaloniaApp()
            .WithInterFont()
            .StartBrowserAppAsync("out");
    }
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}

internal static partial class Interop
{
    [JSImport("startSynth", "Interop")]
    public static partial void StartSynth();

    [JSImport("noteOn", "Interop")]
    public static partial void NoteOn(int channel, int note);

    [JSImport("noteOff", "Interop")]
    public static partial void NoteOff(int channel, int note);

    [JSImport("allNotesOff", "Interop")]
    public static partial void AllNotesOff(int channel);
}

