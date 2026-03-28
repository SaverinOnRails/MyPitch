using Synthesizer;
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
        //  throw new NotImplementedException();
        Interop.NoteOn(0, note);
    }

    public void Release()
    {
        //throw new NotImplementedException();
    }
}
