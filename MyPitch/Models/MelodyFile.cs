using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyPitch.Models;

public class MelodyFile
{
    public string Title { get; set; } = "Unknown";
    public string Author { get; set; } = "Unknown";
    public double DurationMs { get; set; }

    public List<NoteEvent> NoteEvents { get; set; } = new();

    public string ToJson()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(this, options); 
    }

    public static MelodyFile? FromJsonFile(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize(
                json,
                MelodyFileJsonContext.Default.MelodyFile);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(MelodyFile))]
public partial class MelodyFileJsonContext : JsonSerializerContext
{
}

public class NoteEvent
{
    public required string ScaleDegree { get; set; }
    public required double DurationMs { get; set; }
    public required double TriggerAt { get; set; }
    public required int Octave { get; set; }
}
