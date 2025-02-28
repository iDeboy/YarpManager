using System.Text.Json.Serialization;
using YarpManager.Acme.Attributes;
using YarpManager.Acme.Jws.Jwk;

namespace YarpManager.Acme.Jws;

public sealed class JsonWebSignature<TJwk, TPayload> where TJwk : JsonWebKey {

    [Base64Url]
    [JsonPropertyName("protected")]
    public required JsonWebHeader<TJwk> Protected { get; set; }

    [Base64Url]
    [JsonPropertyName("payload")]
    public TPayload Payload { get; set; }

    [JsonPropertyName("signature")]
    public string Signature { get; set; }

}
