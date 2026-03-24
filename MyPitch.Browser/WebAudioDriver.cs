using Synthesizer;

namespace MyPitch.Browser;

public class WebAudioDriver : IAudioDriver
{
    private MeltySynth.Synthesizer _inner;
    public static int SAMPLE_RATE = 44100;

    public WebAudioDriver(string soundFont)
    {
        _inner = new(soundFont, SAMPLE_RATE);
    }
    public void Play()
    {
        throw new System.NotImplementedException();
    }

    public void Stop()
    {
        throw new System.NotImplementedException();
    }
}
