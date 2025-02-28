using System.Text.Json.Serialization;

namespace YarpManager.Acme.Resources;
public sealed class AcmeChallenge : ExtensionMembers {

    [JsonRequired]
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonRequired]
    [JsonPropertyName("url")]
    public required Uri Url { get; init; }

    [JsonRequired]
    [JsonPropertyName("status")]
    public required AcmeChallengeStatus Status { get; init; }

    [JsonPropertyName("validated")]
    public DateTimeOffset? Validated { get; init; }

    [JsonPropertyName("error")]
    public AcmeError? Error { get; init; }

    [JsonPropertyName("token")]
    public string Token { get; init; } = string.Empty;

}
