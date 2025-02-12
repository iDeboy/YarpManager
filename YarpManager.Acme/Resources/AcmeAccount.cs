using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace YarpManager.Acme.Resources;
public sealed class AcmeAccount {

    /// <summary>
    /// Represents the status of <see cref="AcmeAccount"/>.
    /// </summary>
    public enum StatusType {
        /// <summary>
        /// The valid status.
        /// </summary>
        [EnumMember(Value = "valid")]
        Valid,

        /// <summary>
        /// The deactivated status, initiated by client.
        /// </summary>
        [EnumMember(Value = "deactivated")]
        Deactivated,

        /// <summary>
        /// The revoked status, initiated by server.
        /// </summary>
        [EnumMember(Value = "revoked")]
        Revoked,
    }

    [JsonRequired]
    [JsonPropertyName("status")]
    public required StatusType Status { get; set; }

    [JsonPropertyName("contact")]
    public string[]? Contact { get; set; }

    [JsonPropertyName("termsOfServiceAgreed")]
    public bool? TermsOfServiceAgreed { get; set; } = null;

    // [JsonPropertyName("externalAccountBinding")]
    // public ExternalJsonWebSignature? ExternalAccountBinding { get; set; }

    [JsonRequired]
    [JsonPropertyName("orders")]
    public required Uri Orders { get; set; }

    public sealed class Request {

        [JsonPropertyName("contact")]
        public string[]? Contact { get; set; }

        [JsonPropertyName("termsOfServiceAgreed")]
        public bool? TermsOfServiceAgreed { get; set; } = null;

        [JsonPropertyName("onlyReturnExisting")]
        public bool? OnlyReturnExisting { get; set; } = null;

        // [JsonPropertyName("externalAccountBinding")]
        // public ExternalJsonWebSignature? ExternalAccountBinding { get; set; }

    }

}