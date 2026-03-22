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
public interface IAudioDriver
{
    void Stop();

    void Play();
}

public static class Helper
{
    public static short[] InterlacePcm(float[] left , float[] right)
    {
        var pcm = new short[left.Length * 2];
        for (int i = 0; i < left.Length; i++)
        {
            pcm[i * 2] = (short)(Math.Clamp(left[i], -1f, 1f) * short.MaxValue);
            pcm[i * 2 + 1] = (short)(Math.Clamp(right[i], -1f, 1f) * short.MaxValue);
        }

        return pcm;
    }
}