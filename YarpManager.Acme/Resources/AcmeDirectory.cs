using System.Text.Json.Serialization;

namespace YarpManager.Acme.Resources;
public sealed class AcmeDirectory {

    public readonly struct Metadata {
        public Metadata() { }

        [JsonPropertyName("termsOfService")]
        public Uri? TermsOfService { get; init; }
        [JsonPropertyName("website")]
        public Uri? Website { get; init; }
        [JsonPropertyName("caaIdentities")]
        public string[]? CaaIdentities { get; init; }
        [JsonPropertyName("externalAccountRequired")]
        public bool? ExternalAccountRequired { get; init; } = null;
    }

    [JsonIgnore]
    public Uri? Index { get; internal set; }

    [JsonPropertyName("newNonce")]
    public required Uri NewNonce { get; init; }
    [JsonPropertyName("newAccount")]
    public required Uri NewAccount { get; init; }
    [JsonPropertyName("newOrder")]
    public required Uri NewOrder { get; init; }
    [JsonPropertyName("newAuthz")]
    public Uri? NewAuthz { get; init; }
    [JsonPropertyName("revokeCert")]
    public required Uri RevokeCert { get; init; }
    [JsonPropertyName("keyChange")]
    public required Uri KeyChange { get; init; }
    [JsonPropertyName("meta")]
    public required Metadata Meta { get; init; } = new();

}
