using YarpManager.Acme.Abstractions;
using YarpManager.Acme.Resources;

namespace YarpManager.Acme.Services;
public interface IAuthorizationService {

    IAccountData Account { get; }
    Uri Location { get; }

    ValueTask<AcmeResponse<AcmeAuthorization>> Authorization();

    ValueTask<AcmeResponse<IEnumerable<IChallengeService>>> Challenges();

    ValueTask<AcmeResponse<AcmeAuthorization>> Deactivate();

}
