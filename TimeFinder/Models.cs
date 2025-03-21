using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[JsonConverter(typeof(SegmentConverter))]
public class Segment
{
    public long startTimeMs { get; set; }
    public long endTimeMs { get; set; }
    public string defaultNext { get; set; }
    public UI ui { get; set; }
    public List<Choice> choices { get; set; }
}

public class Choice
{
    public string segmentId { get; set; }
    public string text { get; set; }
    public Dictionary<string, object> impressionData { get; set; } = new Dictionary<string, object>();
}

public class UI
{
    public List<long[]> interactionZones { get; set; } = new List<long[]>();
}

public class Manifest
{
    public int viewableId { get; set; }
    public Dictionary<string, Segment> segments { get; set; }
    public string initialSegment { get; set; }
}

public class Precondition
{
    public string Operator { get; set; }
    public List<object> Conditions { get; set; }
}

public class SegmentConverter : JsonConverter<Segment>
{
    public override void WriteJson(JsonWriter writer, Segment value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("startTimeMs");
        writer.WriteValue(value.startTimeMs);
        writer.WritePropertyName("endTimeMs");
        writer.WriteValue(value.endTimeMs);
        writer.WritePropertyName("defaultNext");
        writer.WriteValue(value.defaultNext);

        if (value.ui != null && value.ui.interactionZones.Count > 0)
        {
            writer.WritePropertyName("ui");
            serializer.Serialize(writer, value.ui);
        }

        if (value.choices != null && value.choices.Count > 0)
        {
            writer.WritePropertyName("choices");
            serializer.Serialize(writer, value.choices);
        }

        writer.WriteEndObject();
    }

    public override Segment ReadJson(JsonReader reader, Type objectType, Segment existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var segment = new Segment();
        serializer.Populate(reader, segment);
        return segment;
    }
}
