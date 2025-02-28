using YarpManager.Acme.Abstractions;
using YarpManager.Acme.Clients;
using YarpManager.Acme.Jws;
using YarpManager.Acme.Resources;
using YarpManager.Acme.Utils;

namespace YarpManager.Acme.Services;
internal sealed class AuthorizationService : IAuthorizationService {

    private readonly IAcmeClient _client;

    public IAccountData Account { get; }

    public Uri Location { get; }

    public AuthorizationService(IAcmeClient client, IAccountData account, Uri location) {
        _client = client;
        Account = account;
        Location = location;
    }

    public ValueTask<AcmeResponse<AcmeAuthorization>> Authorization()
        => _client.Post<AcmeAuthorization, EmptyPayload>(
                Location, Account.Location, Account.KeyInfo, default);

    public async ValueTask<AcmeResponse<IEnumerable<IChallengeService>>> Challenges() {

        var challengesRes = await Authorization().Get(a => a.Challenges);

        if (!challengesRes.TryGet(out var challenges))
            return challengesRes.To<IEnumerable<IChallengeService>>();

        return challengesRes.To(GetChallengeServiceEnumerable, this);
    }

    private static IEnumerable<IChallengeService> GetChallengeServiceEnumerable(AcmeResponse<AcmeChallenge[]> res, AuthorizationService authorizationService) {

        var challenges = res.Content.Value;

        foreach (var challenge in challenges)
            yield return new ChallengeService(authorizationService._client, authorizationService.Account,
                challenge.Url, challenge.Type, challenge.Token);

    }

    public ValueTask<AcmeResponse<AcmeAuthorization>> Deactivate() {

        return _client.Post<AcmeAuthorization, AcmeAuthorization.DeactivationRequest>(
            Location, Account.Location, Account.KeyInfo,
            new AcmeAuthorization.DeactivationRequest {
                Status = AcmeAuthorizationStatus.Deactivated
            });
    }


}
