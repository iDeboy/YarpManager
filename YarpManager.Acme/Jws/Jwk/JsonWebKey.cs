using System.Text.Json.Serialization;

namespace YarpManager.Acme.Jws.Jwk;
public abstract class JsonWebKey {

    [JsonPropertyName("kty")]
    [JsonPropertyOrder(2)]
    public KeyType KeyType { get; set; }

    protected JsonWebKey(KeyType kty) {
        KeyType = kty;
    }

}
