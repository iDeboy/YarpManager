using System.Text.Json.Serialization;

namespace YarpManager.Acme.Resources;
public class AcmeIdentifier : ExtensionMembers {


    [JsonPropertyName("type")]
    [JsonRequired]
    public required AcmeIdentifierType Type { get; init; }

    [JsonPropertyName("value")]
    [JsonRequired]
    public required string Value { get; init; }

}
