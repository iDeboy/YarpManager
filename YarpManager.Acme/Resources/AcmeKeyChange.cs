using System.Text.Json.Serialization;
using YarpManager.Acme.Jws;
using YarpManager.Acme.Jws.Jwk;

namespace YarpManager.Acme.Resources;
public sealed class AcmeKeyChange<Jwk> : ExtensionMembers where Jwk : JsonWebKey {

    [JsonRequired]
    [JsonPropertyName("account")]
    public required Uri Account { get; init; }

    [JsonRequired]
    [JsonPropertyName("oldKey")]
    public required Jwk OldKey { get; init; }

}
