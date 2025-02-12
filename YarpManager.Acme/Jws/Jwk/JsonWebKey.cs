using System.Text.Json.Serialization;

namespace YarpManager.Acme.Jws.Jwk;
public abstract class JsonWebKey {

    [JsonPropertyName("kty")]
    [JsonPropertyOrder(2)]
    public abstract KeyType KeyType { get; set; }

}
