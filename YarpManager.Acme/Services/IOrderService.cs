using System.Security.Cryptography.X509Certificates;
using YarpManager.Acme.Abstractions;
using YarpManager.Acme.Resources;

namespace YarpManager.Acme.Services;
public interface IOrderService {

    IAccountData Account { get; }

    Uri Location { get; }

    ValueTask<AcmeResponse<AcmeOrder>> Order();

    ValueTask<AcmeResponse<IEnumerable<IAuthorizationService>>> Authorizations();

    ValueTask<AcmeResponse<AcmeOrder>> Finalize(CsrInfo csrInfo);

    ValueTask<AcmeResponse<X509Certificate2>> Download();
}
