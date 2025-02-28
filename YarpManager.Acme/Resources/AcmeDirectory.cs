using System.Text.Json.Serialization;

namespace YarpManager.Acme.Resources;
public sealed partial class AcmeDirectory : ExtensionMembers {

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
    public required Metadata? Meta { get; init; }

}
