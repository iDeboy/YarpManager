using System.Text.Json.Serialization;
using YarpManager.Acme.Jws.Jwk;

namespace YarpManager.Acme.Jws;

public sealed class JsonWebHeader<TJwk> where TJwk : JsonWebKey {

    [JsonPropertyName("alg")]
    public JsonSignAlgorithm Algorithm { get; set; }

    [JsonPropertyName("nonce")]
    public string Nonce { get; set; } = default!;

    [JsonPropertyName("url")]
    public Uri Url { get; set; } = default!;

    [JsonPropertyName("jwk")]
    public TJwk? JsonWebKey { get; set; }

    [JsonPropertyName("kid")]
    public Uri? KeyId { get; set; }

}
