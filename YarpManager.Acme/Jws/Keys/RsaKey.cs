using System.Security.Cryptography;

namespace YarpManager.Acme.Jws.Keys;
public sealed class RsaKey : AsymmetricKey {
    public override RSA Key { get; }

    public RsaKey(RSA rsa) {
        Key = rsa;
    }

    public RsaKey(RSAParameters parameters) {
        Key = RSA.Create(parameters);
    }

}
