using System.Text.Json.Serialization;

namespace YarpManager.Acme.Resources;
public sealed class Metadata : ExtensionMembers {

    [JsonPropertyName("termsOfService")]
    public Uri? TermsOfService { get; init; }
    [JsonPropertyName("website")]
    public Uri? Website { get; init; }
    [JsonPropertyName("caaIdentities")]
    public string[]? CaaIdentities { get; init; }
    [JsonPropertyName("externalAccountRequired")]
    public bool ExternalAccountRequired { get; init; }
}