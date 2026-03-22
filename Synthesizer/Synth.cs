using MeltySynth;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Synthesizer;

public class Synthesizer
{
    public static int SAMPLE_RATE = 44100;
    private MeltySynth.Synthesizer _inner;
    public Synthesizer()
    {
        _inner = new MeltySynth.Synthesizer(_soundFont, SAMPLE_RATE);
    }
    private string _soundFont => Path.Join(AppDomain.CurrentDomain.BaseDirectory, "default.sf2"); //TEMP, will abstract later

    public void Play()
    {
        _inner.NoteOn(0, 60, 100);
    }

    public void Render(float[] left, float[] right)
    {
        _inner.Render(left, right);
    }
}
public interface IAudioDriver : IDisposable
{
    void Start();
    void Stop();

    void Play();
}