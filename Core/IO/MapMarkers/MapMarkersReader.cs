using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenTemple.Core.IO.MapMarkers
{
    public class MapMarkersReader
    {
        public static PredefinedMapMarkers Load(IFileSystem fs, string path)
        {
            using var memory = fs.ReadFile(path);
            return JsonSerializer.Deserialize<PredefinedMapMarkers>(memory.Memory.Span);
        }
    }

    public class PredefinedMapMarkers
    {
        [JsonPropertyName("markers")]
        public List<PredefinedMapMarker> Markers { get; set; }
    }

    public class PredefinedMapMarker
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("initiallyVisible")]
        public bool InitiallyVisible { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }
    }
}