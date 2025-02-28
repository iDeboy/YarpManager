using System.Text.Json.Serialization;
using YarpManager.Acme.Attributes;

namespace YarpManager.Acme.Resources;
public sealed class AcmeOrder : ExtensionMembers {

    [JsonRequired]
    [JsonPropertyName("status")]
    public required AcmeOrderStatus Status { get; init; }

    [JsonPropertyName("expires")]
    public DateTimeOffset? Expires { get; init; }

    [JsonRequired]
    [JsonPropertyName("identifiers")]
    public required AcmeIdentifier[] Identifiers { get; init; }

    [JsonPropertyName("notBefore")]
    public DateTimeOffset? NotBefore { get; init; }

    [JsonPropertyName("notAfter")]
    public DateTimeOffset? NotAfter { get; init; }

    [JsonPropertyName("error")]
    public ProblemDetails? Error { get; init; }

    [JsonRequired]
    [JsonPropertyName("authorizations")]
    public required Uri[] Authorizations { get; init; }

    [JsonRequired]
    [JsonPropertyName("finalize")]
    public required Uri Finalize { get; init; }

    [JsonPropertyName("certificate")]
    public Uri? Certificate { get; init; }

    internal sealed class Request : ExtensionMembers {

        [JsonRequired]
        [JsonPropertyName("identifiers")]
        public required AcmeIdentifier[] Identifiers { get; init; }

        [JsonPropertyName("notBefore")]
        public DateTimeOffset? NotBefore { get; init; }

        [JsonPropertyName("notAfter")]
        public DateTimeOffset? NotAfter { get; init; }

    }

    internal sealed class FinalizeRequest {

        [JsonRequired]
        [JsonPropertyName("csr")]
        [Base64Url]
        public required byte[] Csr { get; init; }
    }
}
