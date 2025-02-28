using System.Security.Cryptography;
using System.Text.Json.Serialization;
using YarpManager.Acme.Attributes;
using YarpManager.Acme.Utils;

namespace YarpManager.Acme.Jws.Jwk;
public sealed class EcJsonWebKey : JsonWebKey {

    internal ECDsa? _ec;

    [JsonPropertyOrder(1)]
    [JsonPropertyName("crv")]
    public EllipticCurve Curve { get; set; }

    [JsonPropertyOrder(3)]
    [JsonPropertyName("x")]
    [Base64Url]
    public byte[]? X { get; set; }

    [JsonPropertyOrder(4)]
    [JsonPropertyName("y")]
    [Base64Url]
    public byte[]? Y { get; set; }

    public EcJsonWebKey() : base(KeyType.EC) { }
    public EcJsonWebKey(ECDsa ec) : this() {
        var @params = ec.ExportParameters(false);
        Curve = JwsUtils.GetEllipticCurve(@params.Curve);
        X = @params.Q.X;
        Y = @params.Q.Y;
        _ec = ec;
    }
}
