using YarpManager.Acme.Jws.Keys;
using YarpManager.Acme.Resources;

namespace YarpManager.Acme.Services;
internal sealed class AccountService : IAccountService {
    public Uri Location { get; }

    public AsymmetricKey Key { get; private set; }

    public AccountService(Uri location, AsymmetricKey key) {
        Location = location;
        Key = key;
    }

    public ValueTask<AcmeAccount> ChangeKey(AsymmetricKey key) {
        throw new NotImplementedException();
    }

    public ValueTask<AcmeAccount> Deactivate() {
        throw new NotImplementedException();
    }

    public void Dispose() {

        Key.Dispose();

    }
}
