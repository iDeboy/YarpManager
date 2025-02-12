using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace YarpManager.Acme.Jws.Jwk;
public sealed class RsaJsonWebKey : JsonWebKey {

    internal RSA? _rsa;

    [JsonIgnore]
    public override KeyType KeyType { get; set; } = KeyType.RSA;

    [JsonPropertyName("e")]
    [JsonPropertyOrder(1)]
    [Base64Url]
    public byte[]? Exponent { get; set; }

    [JsonPropertyOrder(3)]
    [JsonPropertyName("n")]
    [Base64Url]
    public byte[]? Modulus { get; set; }

    public RsaJsonWebKey() { }
    public RsaJsonWebKey(RSA rsa) {
        var @params = rsa.ExportParameters(false);
        Exponent = @params.Exponent;
        Modulus = @params.Modulus;
        _rsa = rsa;
    }

}
