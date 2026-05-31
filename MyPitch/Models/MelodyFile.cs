using System.Collections.Generic;
using System.Text.Json;

namespace MyPitch.Models;

public class MelodyFile
{
    public string Title { get; set; } = "Unknown";
    public string Author { get; set; } = "Unknown";
    public double DurationMs { get; set; }

    public List<NoteEvent> NoteEvents {get;set;} = new();

    public string ToJson()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(this, options);
    }
}

public class NoteEvent
{
    public required string ScaleDegree { get; set; }
    public required double DurationMs { get; set; }
    public required double TriggerAt { get; set; }
    public required int Octave { get; set; }
}
