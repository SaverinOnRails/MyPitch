using System;
using System.Collections.Generic;
using System.Linq;
namespace MyPitch.Models;

public static class MusicTheory
{
    //TODO: Having string array and Tonic seems stupid
    public static string[] ChromaticScale = new string[] {
        "C",
        "Dflat",
        "D",
        "Eflat",
        "E",
        "F",
        "Gflat",
        "G",
        "Aflat",
        "A",
        "Bflat",
        "B"
    };
    private static readonly string[] FifthIntervalScaleGraduation = {
        "1",
        "5",
        "2",
        "6",
        "3",
        "7",
        "#4",
        "♭2",
        "♭6",
        "♭3",
        "♭7",
        "4"
    };
    public static readonly Dictionary<string, ResolutionData> SimpleResolutionMap = new()
    {
        { "5",  new("1",true) },
        { "2",  new("1") },
        { "6",  new("5") },
        { "3",  new("1") },
        { "7",  new("1",true) },
        { "#4", new("5") },
        { "♭2", new("1") },
        { "♭6", new("5") },
        { "♭3", new("2") },
        { "♭7", new("1",true) },
        { "4",  new("3") }
    };
    public static readonly string[] ChromaticScaleGraduation = {
        "1",
        "♭2",
        "2",
        "♭3",
        "3",
        "4",
        "#4",
        "5",
        "♭6",
        "6",
        "♭7",
        "7"
    };
    public static Key[] Keys = new Key[] {
        Key.C, Key.Dflat, Key.D, Key.Eflat, Key.E, Key.F, Key.Gflat, Key.G, Key.Aflat, Key.A, Key.Bflat, Key.B
    };

    public static string GetDegreeFromCircleOfFifthsIndex(int index) => MusicTheory.FifthIntervalScaleGraduation[index];

    public static int GetIndexFromCircleOfFithsDegree(string degree) => FifthIntervalScaleGraduation.IndexOf(degree);

    private static string NoteAtDegree(Key tonic, int degree, bool correctForFifths = false)
    {
        int tonicIndex = Array.IndexOf(ChromaticScale, tonic.ToString());
        int noteIndex;

        if (!correctForFifths)
        {
            noteIndex = (tonicIndex + (degree - 1)) % ChromaticScale.Length;
        }
        else
        {
            noteIndex = (tonicIndex + 7 * (degree - 1)) % ChromaticScale.Length; //7 semitones for a fifth
        }
        if (noteIndex < 0) noteIndex += ChromaticScale.Length;
        return ChromaticScale[noteIndex];
    }

    public static string GetNoteAtDegree(Key Tonic, string degree)
    {
        int index = MusicTheory.ChromaticScaleGraduation.IndexOf(degree) + 1;
        return NoteAtDegree(Tonic, index, false);
    }

    public static string GetNoteAtCircleOfFifthIndex(Key Tonic, int index)
    {
        index++;
        return NoteAtDegree(Tonic, index, true);
    }

    //zero indexed
    public static int GetCircleOfFifthsIndexFromNote(Key tonic, string note)
    {
        int tonicIndex = Array.IndexOf(ChromaticScale, tonic.ToString());
        int targetIndex = Array.IndexOf(ChromaticScale, note);

        int current = tonicIndex;

        for (int k = 0; k < 12; k++)
        {
            if (current == targetIndex)
                return k;
            current = (current + 7) % 12;
        }
        return -1;
    }
    public static int ToMidiNote(Key tonic, string note, int octave = 4)
    {
        int baseMidiC0 = 12;
        int tonicIndex = Array.IndexOf(ChromaticScale, tonic.ToString());
        int noteIndex = Array.IndexOf(ChromaticScale, note);
        int semitoneOffset = noteIndex - tonicIndex;
        if (semitoneOffset < 0)
            semitoneOffset += 12;
        int tonicMidi = baseMidiC0 + tonicIndex + (octave * 12);
        return tonicMidi + semitoneOffset;
    }

    //same as above but not relative to the tonic
    public static int ToMidiNote(string note, int octave = 4)
    {
        var index = Array.IndexOf(ChromaticScale, note);
        return (octave + 1) * 12 + index;
    }


    public static List<string> GenMelody(List<string> degs, int noteCount , ScaleMode mode)
    {
        var result = new List<string>();
        for (int i = 0; i < noteCount; i++)
        {
            //the first note can be truly random
            if (i == 0)
                result.Add(degs[Random.Shared.Next(degs.Count)]);
            else
                result.Add(NextNote(result[i - 1], mode, degs));
        }
        return result;
    }

    private static string NextNote(string prevNote, ScaleMode scale, List<string> degs)
    {
        var roll = Random.Shared.Next(100);
        string note;
        if (roll < 10)
        {
            note = prevNote;
        }
        else if (roll < 60)
        {
            note = Step(prevNote, scale);
        }
        else if (roll < 90)
        {
            note = Leap(prevNote, scale);
        }
        else
        {
            note = degs[Random.Shared.Next(degs.Count)];
        }
        return degs.Contains(note) ? note : degs[Random.Shared.Next(degs.Count)];
    }

    public static (string RomanNumeral, ChordQuality Quality)? TriadQuality(string scaleDegree, ScaleMode scaleMode)
    {

        var modeData = scaleMode switch
        {
            ScaleMode.Ionian => new[]
            {
            ("1",  "I",   ChordQuality.Major),
            ("2",  "ii",  ChordQuality.Minor),
            ("3",  "iii", ChordQuality.Minor),
            ("4",  "IV",  ChordQuality.Major),
            ("5",  "V",   ChordQuality.Major),
            ("6",  "vi",  ChordQuality.Minor),
            ("7",  "vii°",ChordQuality.Diminished),
            },

            ScaleMode.Dorian => new[]
            {
            ("1",  "i",   ChordQuality.Minor),
            ("2",  "ii",  ChordQuality.Minor),
            ("♭3", "III", ChordQuality.Major),
            ("4",  "IV",  ChordQuality.Major),
            ("5",  "v",   ChordQuality.Minor),
            ("6",  "vi°", ChordQuality.Diminished),
            ("♭7", "VII", ChordQuality.Major),
        },

            ScaleMode.Phrygian => new[]
            {
            ("1",  "i",   ChordQuality.Minor),
            ("♭2", "II",  ChordQuality.Major),
            ("♭3", "III", ChordQuality.Major),
            ("4",  "iv",  ChordQuality.Minor),
            ("5",  "v°",  ChordQuality.Diminished),
            ("♭6", "VI",  ChordQuality.Major),
            ("♭7", "vii", ChordQuality.Minor),
        },

            ScaleMode.Lydian => new[]
            {
            ("1",  "I",   ChordQuality.Major),
            ("2",  "II",  ChordQuality.Major),
            ("3",  "iii", ChordQuality.Minor),
            ("#4", "iv°", ChordQuality.Diminished),
            ("5",  "V",   ChordQuality.Major),
            ("6",  "vi",  ChordQuality.Minor),
            ("7",  "vii", ChordQuality.Minor),
        },

            ScaleMode.Mixolydian => new[]
            {
            ("1",  "I",   ChordQuality.Major),
            ("2",  "ii",  ChordQuality.Minor),
            ("3",  "iii°",ChordQuality.Diminished),
            ("4",  "IV",  ChordQuality.Major),
            ("5",  "v",   ChordQuality.Minor),
            ("6",  "vi",  ChordQuality.Minor),
            ("♭7", "VII", ChordQuality.Major),
        },

            ScaleMode.Aeolian => new[]
            {
            ("1",  "i",   ChordQuality.Minor),
            ("2",  "ii°", ChordQuality.Diminished),
            ("♭3", "III", ChordQuality.Major),
            ("4",  "iv",  ChordQuality.Minor),
            ("5",  "v",   ChordQuality.Minor),
            ("♭6", "VI",  ChordQuality.Major),
            ("♭7", "VII", ChordQuality.Major),
        },

            ScaleMode.Locrian => new[]
            {
            ("1",  "i°",  ChordQuality.Diminished),
            ("♭2", "II",  ChordQuality.Major),
            ("♭3", "iii", ChordQuality.Minor),
            ("4",  "iv",  ChordQuality.Minor),
            ("#4", "V",   ChordQuality.Major), // TODO: #4 is not present in standard locrian. This should display flat 5
            ("♭6", "VI",  ChordQuality.Major),
            ("♭7", "vii", ChordQuality.Minor),
        },

            _ => throw new ArgumentOutOfRangeException(nameof(scaleMode))
        };

        foreach (var chord in modeData)
        {
            if (chord.Item1 == scaleDegree)
                return (chord.Item2, chord.Item3);
        }

        return null;
    }
    public static List<int> BuildChord(Key root, Key Tonic, ChordQuality quality, int? Octave = 4)
    {
        int rootIndex = Array.IndexOf(Keys, root);
        if (rootIndex == -1)
            throw new ArgumentException("Invalid root key.", nameof(root));
        int[] intervals = quality switch
        {
            ChordQuality.Major => new[] { 0, 4, 7 },
            ChordQuality.Minor => new[] { 0, 3, 7 },
            ChordQuality.Diminished => new[] { 0, 3, 6 },
            ChordQuality.Augmented => new[] { 0, 4, 8 },
            ChordQuality.Dominant7 => new[] { 0, 4, 7, 10 },
            ChordQuality.Major7 => new[] { 0, 4, 7, 11 },
            ChordQuality.Minor7 => new[] { 0, 3, 7, 10 },
            _ => throw new ArgumentOutOfRangeException(nameof(quality))
        };
        List<int> chord = new(intervals.Length);
        foreach (int interval in intervals)
        {
            var key = Keys[(rootIndex + interval) % Keys.Length];
            chord.Add(ToMidiNote(Tonic, key.ToString(), Octave != null ? Octave.Value : 4));
        }
        return chord;
    }

    public static List<int>? GetDiatonicChord(Key tonic, string scaleDegree, ScaleMode scaleMode, int octave)
    {
        var triad = TriadQuality(scaleDegree, scaleMode);
        if (triad is null) return null; //not diatonic
        var root = MusicTheory.GetNoteAtDegree(tonic, scaleDegree);
        var rootAsKey = MusicTheory.Keys[Array.IndexOf(MusicTheory.ChromaticScale, root)];
        var chord = MusicTheory.BuildChord(rootAsKey, tonic, triad.Value.Quality, octave);
        return chord;
    }
    private static string Leap(string prevNote, ScaleMode mode)
    {
        //intervals within the key context, not semitones
        var intervals = new int[] { 2, 4, 5 };
        var scale = DegsForScaleMode(mode);
        int interval = intervals[Random.Shared.Next(intervals.Count())];
        int dir = Random.Shared.Next(2) == 0 ? -1 : 1;
        var indexInScale = scale.IndexOf(prevNote);
        var noteIndex = ((indexInScale + interval * dir) % scale.Count + scale.Count) % scale.Count;
        var note = scale[noteIndex];
        return note;
    }

    private static string Step(string prevNote, ScaleMode mode)
    {
        var scale = DegsForScaleMode(mode);
        int dir = Random.Shared.Next(2) == 0 ? -1 : 1;
        var indexInScale = scale.IndexOf(prevNote);
        var noteIndex = ((indexInScale + dir) % scale.Count + scale.Count) % scale.Count; // safe wrap around for negative index
        var note = scale[noteIndex];
        return note;
    }
    private static ScaleMode BestFitScaleMode(List<string> degs)
    {
        ScaleMode bestFit = ScaleMode.Ionian;
        int strength = 0;
        foreach (var mode in Enum.GetValues<ScaleMode>())
        {
            var intersects = DegsForScaleMode(mode).Intersect(degs).Count();
            if (intersects > strength)
            {
                strength = intersects;
                bestFit = mode;
            }
        }
        return bestFit;
    }

    public static List<string> DegsForScaleMode(ScaleMode mode)
    {
        return mode
        switch
        {
            ScaleMode.Ionian => new() {
                    "1",
                    "2",
                    "3",
                    "4",
                    "5",
                    "6",
                    "7"
                },

            ScaleMode.Dorian => new() {
                    "1",
                    "2",
                    "♭3",
                    "4",
                    "5",
                    "6",
                    "♭7"
                },

            ScaleMode.Phrygian => new() {
                    "1",
                    "♭2",
                    "♭3",
                    "4",
                    "5",
                    "♭6",
                    "♭7"
                },

            ScaleMode.Lydian => new() {
                    "1",
                    "2",
                    "3",
                    "#4",
                    "5",
                    "6",
                    "7"
                },

            ScaleMode.Mixolydian => new() {
                    "1",
                    "2",
                    "3",
                    "4",
                    "5",
                    "6",
                    "♭7"
                },

            ScaleMode.Aeolian => new() {
                    "1",
                    "2",
                    "♭3",
                    "4",
                    "5",
                    "♭6",
                    "♭7"
                },

            ScaleMode.Locrian => new() {
                    "1",
                    "♭2",
                    "♭3",
                    "4",
                    "#4", // TODO: #4 is not present in standard locrian. This should display flat 5
                    "♭6",
                    "♭7"
                },
        };
    }
}
public enum Key
{
    C,
    G,
    D,
    A,
    E,
    B,
    Gflat,
    Dflat,
    Aflat,
    Eflat,
    Bflat,
    F
}
public enum ScaleMode
{
    Ionian,
    Dorian,
    Phrygian,
    Lydian,
    Mixolydian,
    Aeolian,
    Locrian
}

public enum ChordQuality
{
    Major,
    Minor,
    Diminished,
    Augmented,
    Dominant7,
    Major7,
    Minor7
}
public class ResolutionData
{
    public string ResolveTo { get; set; }
    public bool ResolveToNextOctave;

    public ResolutionData(string resolveTo, bool resolveToNextOctave = false)
    {
        ResolveTo = resolveTo;
        ResolveToNextOctave = resolveToNextOctave;
    }
}
