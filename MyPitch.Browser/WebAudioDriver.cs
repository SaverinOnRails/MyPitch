using System;
using System.IO;

namespace MyPitch.Browser;

public class WebAudioDriver : IAudioDriver
{
    public WebAudioDriver()
    {
        Interop.StartSynth();
    }
    public void Play(int note)
    {
        Interop.NoteOn(0, note);
    }

    public void PlayDrone(int note)
    {
        Interop.NoteOn(5, note);
    }

    public unsafe void PlaySpeechSample(string sample)
    {
        var stream = EmbeddedResources.Get(sample);
        if (stream is null)
        {
            return;
        }
        // Copy the stream into a byte array
        byte[] wavData;
        using (var ms = new MemoryStream())
        {
            stream.CopyTo(ms);
            wavData = ms.ToArray();
        }
        Interop.PlaySpeechSample(wavData);
    }

    public void Release(int note)
    {
        Interop.NoteOff(0, note);
    }

    public void ReleaseDrone()
    {
        Interop.AllNotesOff(5);
    }
}
