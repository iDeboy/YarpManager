using System.Text.Json.Serialization;

namespace YarpManager.Acme.Resources;
public sealed class AcmeAccount : ExtensionMembers {

    [JsonRequired]
    [JsonPropertyName("status")]
    public required AccountStatusType Status { get; init; }

    [JsonPropertyName("contact")]
    public string[]? Contact { get; init; }

    [JsonPropertyName("termsOfServiceAgreed")]
    public bool? TermsOfServiceAgreed { get; init; }

    // [JsonPropertyName("externalAccountBinding")]
    // public ExternalJsonWebSignature? ExternalAccountBinding { get; init; }

    [JsonPropertyName("orders")]
    public Uri? Orders { get; init; }

    internal sealed class Request : ExtensionMembers {

        [JsonPropertyName("contact")]
        public string[]? Contact { get; set; }

        [JsonPropertyName("termsOfServiceAgreed")]
        public bool? TermsOfServiceAgreed { get; set; }

        [JsonPropertyName("onlyReturnExisting")]
        public bool OnlyReturnExisting { get; set; }

        // [JsonPropertyName("externalAccountBinding")]
        // public ExternalJsonWebSignature? ExternalAccountBinding { get; set; }

    }

}