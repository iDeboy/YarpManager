using System.Security.Cryptography;

namespace YarpManager.Acme.Jws.Keys;
public sealed class EcKey : AsymmetricKey {
    public override ECDsa Key { get; }

    public EcKey(ECDsa ec) {
        Key = ec;
    }

    public EcKey(ECParameters parameters) {
        Key = ECDsa.Create(parameters);
    }
}
