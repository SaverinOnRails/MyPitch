using Android.Media;
using Synthesizer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AndroidMedia = Android.Media;
namespace MyPitch.Android;

internal class AudioTrackDriver : IAudioDriver
{
    public static int SAMPLE_RATE = 44100;

    private MeltySynth.Synthesizer _inner;


    public AudioTrackDriver(string soundFont)
    {
        _inner = new MeltySynth.Synthesizer(soundFont, SAMPLE_RATE);
    }

    public void Play()
    {
       
    }

    public void Stop()
    {
       
    }
}