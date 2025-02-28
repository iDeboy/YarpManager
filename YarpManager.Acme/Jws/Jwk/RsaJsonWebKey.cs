using System.Security.Cryptography;
using System.Text.Json.Serialization;
using YarpManager.Acme.Attributes;

namespace YarpManager.Acme.Jws.Jwk;
public sealed class RsaJsonWebKey : JsonWebKey {

    internal RSA? _rsa;

    [JsonPropertyName("e")]
    [JsonPropertyOrder(1)]
    [Base64Url]
    public byte[]? Exponent { get; set; }

    [JsonPropertyOrder(3)]
    [JsonPropertyName("n")]
    [Base64Url]
    public byte[]? Modulus { get; set; }

    public RsaJsonWebKey() : base(KeyType.RSA) { }
    public RsaJsonWebKey(RSA rsa) : this() {
        var @params = rsa.ExportParameters(false);
        Exponent = @params.Exponent;
        Modulus = @params.Modulus;
        _rsa = rsa;
    }

}
