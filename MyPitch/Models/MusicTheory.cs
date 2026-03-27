using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
namespace MyPitch.Models.MusicTheory;

internal static class MusicTheory
{
    //TODO: Having string array and Tonic seems stupid
    private static string[] _chromaticScale = new string[] { "C", "Dflat", "D", "Eflat", "E", "F", "Gflat", "G", "Aflat", "A", "Bflat", "B" };
    public static string NoteAtDegree(Key tonic, int degree, bool correctForFifths = false)
    {
        int tonicIndex = Array.IndexOf(_chromaticScale, tonic.ToString());
        int noteIndex;

        if (!correctForFifths)
        {
            noteIndex = (tonicIndex + (degree - 1)) % _chromaticScale.Length;
        }
        else
        {
            noteIndex = (tonicIndex + 7 * (degree - 1)) % _chromaticScale.Length; //7 semitones for a fifth
        }
        if (noteIndex < 0) noteIndex += _chromaticScale.Length;
        return _chromaticScale[noteIndex];
    }

    public static int ToMidiNote(string tonic, string note, int octave = 4)
    {
        int baseMidiC0 = 12;
        int tonicIndex = Array.IndexOf(_chromaticScale, tonic);
        int noteIndex = Array.IndexOf(_chromaticScale, note);
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

