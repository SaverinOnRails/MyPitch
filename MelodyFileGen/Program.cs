using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using MyPitch.Models;

class Program
{
    private static string FilePath = string.Empty;
    private static string Tonic = string.Empty;
    private static int Channel = 0;
    private static string Title = "Unknown";

    public static void Main(String[] args)
    {
        int index = 0;
        var stringChannel = String.Empty;
        foreach (var arg in args)
        {
            if (args.Length - 1 == index) break;
            switch (arg)
            {
                case "--file":
                    FilePath = args[index + 1];
                    break;
                case "--tonic":
                    Tonic = args[index + 1];
                    break;
                case "--channel":
                    stringChannel = args[index + 1];
                    break;
                case "--title":
                    Title = args[index + 1];
                    break;

            }
            index++;
        }
        if (FilePath == string.Empty || Tonic == String.Empty || stringChannel == String.Empty) throw new Exception("Incomplete Arguments");
        try
        {
            Tonic = NormalizeNoteName(Tonic);
        }
        catch
        {
            throw new Exception("Invalid Tonic Provided");
        }

        try
        {
            Channel = int.Parse(stringChannel);
        }
        catch
        {
            throw new Exception("Invalid Channel Provided");
        }
        Generate();
    }

    private static void Generate()
    {
        MelodyFile melodyFile = new();
        melodyFile.Title = Title;
        var midiFile = MidiFile.Read(FilePath);
        melodyFile.DurationMs = midiFile.GetDuration<MetricTimeSpan>().TotalMilliseconds;
        var tempoMap = midiFile.GetTempoMap();
        var notes = midiFile.GetNotes().Where(p => p.Channel == Channel);
        foreach (var note in notes)
        {
            ScaleDegree(note.NoteName);
            melodyFile.NoteEvents.Add(new()
            {
                ScaleDegree = ScaleDegree(note.NoteName),
                DurationMs = note.LengthAs<MetricTimeSpan>(tempoMap).TotalMilliseconds,
                TriggerAt = note.TimeAs<MetricTimeSpan>(tempoMap).TotalMilliseconds,
                Octave = note.Octave,
            });
        }
        var json = melodyFile.ToJson();
        var file = Path.GetFileNameWithoutExtension(FilePath) + ".json";
        File.WriteAllText(file, json);
        Console.WriteLine($"Completed with {melodyFile.NoteEvents.Count()} notes. Written to '{file}'");
    }

    private static string NormalizeNoteName(NoteName noteName)
    {
        switch (noteName)
        {
            case NoteName.CSharp: return "Dflat";
            case NoteName.DSharp: return "Eflat";
            case NoteName.FSharp: return "Gflat";
            case NoteName.GSharp: return "Aflat";
            case NoteName.ASharp: return "Bflat";
            default: return noteName.ToString();
        }
    }
    private static string NormalizeNoteName(string noteName)
    {
        var note = noteName.ToUpper();
        string normalNote = note switch
        {
            "C#" => "Dflat",
            "D#" => "Eflat",
            "F#" => "Gflat",
            "G#" => "Aflat",
            "Asharp" => "Bflat",
            _ => note,
        };

        if (!MusicTheory.ChromaticScale.Contains(normalNote))
        {
            throw new Exception();
        }
        return normalNote;
    }

    //convert to scale degree based on tonic note
    private static string ScaleDegree(NoteName noteName)
    {
        var note = NormalizeNoteName(noteName);
        var tonicIndex = MusicTheory.ChromaticScale.IndexOf(Tonic);
        var tonicScale = MusicTheory.ChromaticScale.Skip(tonicIndex).Concat(MusicTheory.ChromaticScale.Take(tonicIndex)).ToList();
        var scaleDegree = MusicTheory.ChromaticScaleGraduation[tonicScale.IndexOf(note)];
        scaleDegree = scaleDegree.Replace("♭", "flat");
        return scaleDegree;
    }
}
