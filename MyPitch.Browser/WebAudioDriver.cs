using MeltySynth;
using Synthesizer;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MyPitch.Browser;

public class WebAudioDriver : IAudioDriver
{
    private MeltySynth.Synthesizer _inner;
    public static int SAMPLE_RATE = 44100;
    private float[] _buffer;
    public WebAudioDriver()
    {
        Interop.StartAudioServer();
        var blocksize = 1024;
        _buffer = new float[blocksize * 2];

    }
    private void ConfigureSynth(MemoryStream data)
    {
        var sf = new SoundFont(data);
        _inner = new(sf, SAMPLE_RATE);
    }
    public void Play()
    {
        _inner.NoteOn(0, 60, 100);
       // _inner.NoteOffAll(false);
    }

    public unsafe nint WriteToSink()
    {
        _inner.RenderInterleaved(_buffer);
        fixed (float* p = _buffer)
        {
            return (nint)p;
           // Interop.PostAudio((nint)p, _buffer.Length);
        }
    }

    public void Stop()
    {
        throw new System.NotImplementedException();
    }

    public static async Task<WebAudioDriver> CreateAsync(HttpClient http)
    {
        var data = await http.GetByteArrayAsync("soundfonts/default.sf2");

        var driver = new WebAudioDriver();
        using var ms = new MemoryStream(data);
        driver.ConfigureSynth(ms);

        return driver;
    }
}
