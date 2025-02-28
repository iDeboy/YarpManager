using System.Text.Json.Serialization;

namespace YarpManager.Acme.Resources;
public class AcmeErrorIdentifier : ExtensionMembers {


    [JsonPropertyName("type")]
    [JsonRequired]
    public required string Type { get; init; }

    [JsonPropertyName("value")]
    [JsonRequired]
    public required string Value { get; init; }

}
