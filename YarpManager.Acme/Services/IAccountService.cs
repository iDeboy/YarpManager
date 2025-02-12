using System.Security.Principal;
using YarpManager.Acme.Jws.Keys;
using YarpManager.Acme.Resources;

namespace YarpManager.Acme.Services;
public interface IAccountService : IDisposable {

    Uri Location { get; }
    AsymmetricKey Key { get; }

    ValueTask<AcmeAccount> Deactivate();

    ValueTask<AcmeAccount> ChangeKey(AsymmetricKey key);

}
