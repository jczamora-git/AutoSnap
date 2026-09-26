using System.Text.Json.Serialization;

namespace AutoSnap.BrowserCapture;

public class BrowserCaptureMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("requestId")]
    public int? RequestId { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("data")]
    public string? Data { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("quality")]
    public double? Quality { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
