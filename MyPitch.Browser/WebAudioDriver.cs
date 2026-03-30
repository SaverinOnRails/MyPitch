using System;

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

    public void Release(int note)
    {
        Interop.NoteOff(0,note);
    }

    public void ReleaseDrone()
    {
        Interop.AllNotesOff(5);
      }
}
