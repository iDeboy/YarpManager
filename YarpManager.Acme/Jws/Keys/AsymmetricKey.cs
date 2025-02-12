using System.Security.Cryptography;
using YarpManager.Acme.Utils;

namespace YarpManager.Acme.Jws.Keys;
public abstract class AsymmetricKey : IDisposable {

    public static AsymmetricKey Create(JsonSignAlgorithm algorithm) {

        return algorithm switch {
            JsonSignAlgorithm.RS256 or
            JsonSignAlgorithm.RS384 or
            JsonSignAlgorithm.RS512 or
            JsonSignAlgorithm.PS256 or
            JsonSignAlgorithm.PS384 or
            JsonSignAlgorithm.PS512 => new RsaKey(RSA.Create()) {
                Algorithm = algorithm,
            },
            JsonSignAlgorithm.ES256 or
            JsonSignAlgorithm.ES384 or
            JsonSignAlgorithm.ES512 => new EcKey(ECDsa.Create(JwsUtils.GetEllipticCurveName(algorithm))) {
                Algorithm = algorithm,
            },
            _ => throw new NotSupportedException($"Unsuported algorithm {algorithm}."),
        };

    }

    public JsonSignAlgorithm Algorithm { get; set; }
    public abstract AsymmetricAlgorithm Key { get; }

    public void Dispose() {

        Key.Dispose();

        GC.SuppressFinalize(this);
    }

}
