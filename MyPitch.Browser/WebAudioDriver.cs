using System.IO;
using System.Collections.Generic;

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

    public void PlayDrone(int note , int velocity)
    {
        Interop.NoteOn(5, note, velocity);
    }

    public void PlayChord(List<int> notes)
    {
        ReleaseAll();
        foreach (int note in notes)
        {
            Interop.NoteOn(0, note, 127);
        }
    }
    public void PlaySpeechSample(string sample)
    {
        var stream = EmbeddedResources.GetSpeechSample(sample);
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

    public void ReleaseAll()
    {
        Interop.AllNotesOff(0);
    }

    public void ReleaseDrone()
    {
        Interop.AllNotesOff(5);
    }
}
