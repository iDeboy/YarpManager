using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using YarpManager.Acme.Jws.Jwk;
using YarpManager.Acme.Utils;

namespace YarpManager.Acme.Jws.Keys;
public sealed class RsaKeyInfo : AsymmetricKeyInfo {

    private readonly Lazy<RsaJsonWebKey> _jwk;
    private readonly Lazy<byte[]> _thumbprint;

    public override RSA Key { get; }

    public override RsaJsonWebKey JsonWebKey => _jwk.Value;

    public override byte[] Thumbprint => _thumbprint.Value;

    public RsaKeyInfo(RSA rsa, JsonSignAlgorithm algorithm) : base(algorithm) {
        Key = rsa;
        _jwk = new(CreateJwk, LazyThreadSafetyMode.ExecutionAndPublication);
        _thumbprint = new(CreateThumbprint, LazyThreadSafetyMode.ExecutionAndPublication);
    }


    public RsaKeyInfo(RSAParameters parameters, JsonSignAlgorithm algorithm) : base(algorithm) {
        Key = RSA.Create(parameters);
        _jwk = new(CreateJwk, LazyThreadSafetyMode.ExecutionAndPublication);
        _thumbprint = new(CreateThumbprint, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private byte[] CreateThumbprint() => JwsUtils.GetThumbprint(JsonWebKey);
    private RsaJsonWebKey CreateJwk() => new(Key);

    public override CertificateRequest CreateEmptyCertificateRequest(X500DistinguishedName subjectName) {

        var (rsaPadding, hashName) = JwsUtils.GetGetRSASignaturePaddingAndHashAlgorithmName(Algorithm);

        return new(subjectName, Key, hashName, rsaPadding!);
    }
}
