using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public static readonly string[] FifthIntervalScaleGraduation = {
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

    public static string NoteAtDegree(Key tonic, int degree, bool correctForFifths = false)
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

    //zero indexed
    public static int FifthSegment(Key tonic, string note)
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

    public static List<string> GenMelody(List<string> degs, int noteCount)
    {
        var result = new List<string>();
        var bestfit = BestFitScaleMode(degs);
        for (int i = 0; i < noteCount; i++)
        {
            //the first note can be truly random
            if (i == 0)
                result.Add(degs[Random.Shared.Next(degs.Count)]);
            else
                result.Add(NextNote(result[i - 1], bestfit, degs));
        }
        return result;
    }
    private static string NextNote(string prevNote, ScaleMode scale, List<string> degs)
    {
        var roll = Random.Shared.Next(100);
        Debug.WriteLine(roll);
        string note;
        if(roll < 20)
        {
            note = prevNote;
            Debug.WriteLine($"Repeating {note}");
        }
        if (roll < 50)
        {
            note = Step(prevNote, scale);
            Debug.WriteLine($"Stepping to {prevNote} from {note}");
        }
        else if (roll < 90)
        {
            note = Leap(prevNote, scale);
            Debug.WriteLine($"Leaping to {prevNote} from {note}");
        }
        else
        {
            note = degs[Random.Shared.Next(degs.Count)];
            Debug.WriteLine($"Random roll {note}");

        }
        Debug.WriteLine("");
        return degs.Contains(note) ? note : degs[Random.Shared.Next(degs.Count)];
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
                    "♭5",
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