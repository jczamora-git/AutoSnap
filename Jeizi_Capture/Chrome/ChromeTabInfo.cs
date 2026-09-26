using System.Text.Json.Serialization;

namespace AutoSnap.Chrome;

public sealed class ChromeTabInfo
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("webSocketDebuggerUrl")]
    public string WebSocketDebuggerUrl { get; init; } = string.Empty;

    [JsonPropertyName("faviconUrl")]
    public string FaviconUrl { get; init; } = string.Empty;

    public override string ToString() => string.IsNullOrWhiteSpace(Title) ? Url : $"{Title} ({Url})";
}
