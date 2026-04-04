using System;

namespace MyPitch.Models;

internal static class MusicTheory
{
    //TODO: Having string array and Tonic seems stupid
    public static string[] ChromaticScale = new string[] { "C", "Dflat", "D", "Eflat", "E", "F", "Gflat", "G", "Aflat", "A", "Bflat", "B" };
    public static readonly string[] FifthIntervalScaleGraduation = { "1", "5", "2", "6", "3", "7", "#4", "♭2", "♭6", "♭3", "♭7", "4" };
    public static readonly string[] ChromaticScaleGraduation = { "1", "♭2", "2", "♭3", "3", "4", "#4", "5", "♭6", "6", "♭7", "7" };
    public static Key[] Keys = new Key[] { Key.C, Key.Dflat, Key.D, Key.Eflat, Key.E, Key.F, Key.Gflat, Key.G, Key.Aflat, Key.A, Key.Bflat, Key.B };

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
    public static int ToMidiNote(string tonic, string note, int octave = 4)
    {
        int baseMidiC0 = 12;
        int tonicIndex = Array.IndexOf(ChromaticScale, tonic);
        int noteIndex = Array.IndexOf(ChromaticScale, note);
        int semitoneOffset = noteIndex - tonicIndex;
        if (semitoneOffset < 0)
            semitoneOffset += 12;
        int tonicMidi = baseMidiC0 + tonicIndex + (octave * 12);
        return tonicMidi + semitoneOffset;
    }
}

public enum Key
{
    C, G, D, A, E, B, Gflat, Dflat, Aflat, Eflat, Bflat, F
}

