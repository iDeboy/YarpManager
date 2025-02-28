using YarpManager.Acme.Jws.Keys;

namespace YarpManager.Acme.Abstractions;
public interface IAccountData {
    Uri Location { get; }
    AsymmetricKeyInfo KeyInfo { get; }
}
