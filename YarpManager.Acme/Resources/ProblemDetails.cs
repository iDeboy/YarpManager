using System.Net;
using System.Text.Json.Serialization;

namespace YarpManager.Acme.Resources;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc7807#section-3.1
/// </summary>
public sealed class ProblemDetails : ExtensionMembers {

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("status")]
    public HttpStatusCode Status { get; init; }

    [JsonPropertyName("detail")]
    public string? Detail { get; init; }

    [JsonPropertyName("instance")]
    public Uri? Instance { get; init; }

}
