using System.Text.Json.Serialization;

namespace YarpManager.Acme.Resources;
public sealed class AcmeAuthorization : ExtensionMembers {

    [JsonRequired]
    [JsonPropertyName("identifier")]
    public required AcmeIdentifier Identifier { get; init; }

    [JsonRequired]
    [JsonPropertyName("status")]
    public required AcmeAuthorizationStatus Status { get; init; }

    [JsonPropertyName("expires")]
    public DateTimeOffset? Expires { get; init; }

    [JsonRequired]
    [JsonPropertyName("challenges")]
    public required AcmeChallenge[] Challenges { get; init; }

    [JsonPropertyName("wildcard")]
    public bool? Wildcard { get; init; }

    internal sealed class DeactivationRequest {

        [JsonRequired]
        [JsonPropertyName("status")]
        public required AcmeAuthorizationStatus Status { get; init; }
    }

}
