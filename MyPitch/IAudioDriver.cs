using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MyPitch;

public interface IAudioDriver
{
    void Play(int note);

    void Release(int note);

    void ReleaseAll();

    void PlayDrone(int note, int velocity);

    void PlaySpeechSample(string sample);

    void ReleaseDrone();
}


public static class EmbeddedResources
{
    public static Stream? GetSpeechSample(string name) //Will only look for out WAV files, anything else will crash
    {
        var assembly = Assembly.GetExecutingAssembly();
        var lookUp = name.Replace("♭", "flat-").Replace("#", "sharp-");
        var resource = $"MyPitch.SpeechSamples.{lookUp}.wav";
        var stream = assembly.GetManifestResourceStream(
                 resource);
        return stream;
    }

    public static List<string> GetMelodyFiles()
    {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly
            .GetManifestResourceNames()
            .Where(x => x.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public static string? GetMelodyFile(string path)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(path);
        if (stream == null) return null;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
