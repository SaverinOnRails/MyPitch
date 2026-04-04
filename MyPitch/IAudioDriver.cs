using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace MyPitch;

public interface IAudioDriver
{
    void Play(int note);

    void Release(int note);

    void PlayDrone(int note);

    void PlaySpeechSample(string sample);

    void ReleaseDrone();
}


public static class EmbeddedResources
{
    public static Stream? Get(string name) //Will only look for out WAV files, anything else will crash
    {
        var assembly = Assembly.GetExecutingAssembly();
        var lookUp = name.Replace("♭", "flat-").Replace("#", "sharp-");
        var resource = $"MyPitch.SpeechSamples.{lookUp}.wav";
        var stream = assembly.GetManifestResourceStream(
                 resource);
        return stream;
    }
}