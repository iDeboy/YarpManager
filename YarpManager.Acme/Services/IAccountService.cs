using System.Security.Principal;
using YarpManager.Acme.Abstractions;
using YarpManager.Acme.Jws.Keys;
using YarpManager.Acme.Resources;

namespace YarpManager.Acme.Services;
public interface IAccountService : IAccountData, IDisposable {

    ValueTask<AcmeResponse<AcmeAccount>> Account();
    ValueTask<AcmeResponse<AcmeAccount>> Deactivate();
    ValueTask<AcmeResponse<AcmeAccount>> ChangeKey(AsymmetricKeyInfo key);
    ValueTask<AcmeResponse<IOrderService>> NewOrder(string[] domains, DateTimeOffset? notBefore = null, DateTimeOffset? notAfter = null);

    bool SaveKey(string path);

}
