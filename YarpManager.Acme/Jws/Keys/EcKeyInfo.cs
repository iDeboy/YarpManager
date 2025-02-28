using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using YarpManager.Acme.Jws.Jwk;
using YarpManager.Acme.Utils;

namespace YarpManager.Acme.Jws.Keys;
public sealed class EcKeyInfo : AsymmetricKeyInfo {

    private readonly Lazy<EcJsonWebKey> _jwk;
    private readonly Lazy<byte[]> _thumbprint;

    public override ECDsa Key { get; }

    public override EcJsonWebKey JsonWebKey => _jwk.Value;

    public override byte[] Thumbprint => _thumbprint.Value;

    public EcKeyInfo(ECDsa ec, JsonSignAlgorithm algorithm) : base(algorithm) {
        Key = ec;
        _jwk = new(CreateJwk, LazyThreadSafetyMode.ExecutionAndPublication);
        _thumbprint = new(CreateThumbprint, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public EcKeyInfo(ECParameters parameters, JsonSignAlgorithm algorithm) : base(algorithm) {
        Key = ECDsa.Create(parameters);
        _jwk = new(CreateJwk, LazyThreadSafetyMode.ExecutionAndPublication);
        _thumbprint = new(CreateThumbprint, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private EcJsonWebKey CreateJwk() => new(Key);
    private byte[] CreateThumbprint() => JwsUtils.GetThumbprint(JsonWebKey);

    public override CertificateRequest CreateEmptyCertificateRequest(X500DistinguishedName subjectName)
        => new(subjectName, Key, JwsUtils.GetHashAlgorithmName(Algorithm));

}
