using System;
using System.Collections.Generic;
using System.IO;

namespace MyPitch.Models;

public class MelodyFile
{
    public double DurationMs { get; set; }
    public required string OriginalTonic { get; set; }

    public List<NoteEvent> NoteEvents { get; set; } = new();

    //this of course only works after tonic and scale degrees have been normalised which they will be in MelodyFileGen/Program.cs
    public byte[] Serialize()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        //write duration
        writer.Write(DurationMs);
        //write original tonic
        writer.Write((byte)MusicTheory.ChromaticScale.IndexOf(OriginalTonic));
        //write length of note events
        writer.Write(NoteEvents.Count);

        //write events
        foreach (var ev in NoteEvents)
        {
            //Pack scale degree and octave into one byte
            //First 4 bits for scale degree
            var scaleDeg = ev.ScaleDegree.Replace("flat", "♭");
            byte pack = (byte)MusicTheory.ChromaticScaleGraduation.IndexOf(scaleDeg);
            pack = (byte)(pack << 4);
            //last 4 bits for octave
            pack |= (byte)ev.Octave;
            writer.Write(pack);

            writer.Write(ev.DurationMs);
            writer.Write(ev.TriggerAt);
        }
        return ms.ToArray();
    }
    public static MelodyFile? FromBytes(byte[] buffer)
    {
        try
        {
            var melodyFile = new MelodyFile() { OriginalTonic = "" };
            using var ms = new MemoryStream(buffer);
            using var reader = new BinaryReader(ms);

            //read duration
            melodyFile.DurationMs = reader.ReadDouble();
            //read original tonic
            melodyFile.OriginalTonic = MusicTheory.ChromaticScale[reader.ReadByte()];
            //read length of note events
            int length = reader.ReadInt32();
            for (int i = 0; i < length; i++)
            {
                var pack = reader.ReadByte();
                var octave = (pack & 0x0F);
                pack = (byte)(pack >> 4);
                var scaleDeg = MusicTheory.ChromaticScaleGraduation[pack];
                var duration = reader.ReadDouble();
                var triggerAt = reader.ReadDouble();
                melodyFile.NoteEvents.Add(new()
                {
                    ScaleDegree = scaleDeg,
                    DurationMs = duration,
                    TriggerAt = triggerAt,
                    Octave = octave
                });
            }
            return melodyFile;
        }
        catch
        {
            return null;
        }
    }
}
public class NoteEvent
{
    public required string ScaleDegree { get; set; }
    public required double DurationMs { get; set; }
    public required double TriggerAt { get; set; }
    public required int Octave { get; set; }
}
