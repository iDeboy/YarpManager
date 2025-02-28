using YarpManager.Acme.Abstractions;
using YarpManager.Acme.Resources;

namespace YarpManager.Acme.Services;
public interface IChallengeService {

    IAccountData Account { get; }

    Uri Location { get; }

    string Type { get; }

    string Token { get; }

    string KeyAuthorization { get; }

    ValueTask<AcmeResponse<AcmeChallenge>> Challenge();

    ValueTask<AcmeResponse<AcmeChallenge>> Validate();
}
