using YarpManager.Acme.Abstractions;
using YarpManager.Acme.Clients;
using YarpManager.Acme.Jws;
using YarpManager.Acme.Resources;

namespace YarpManager.Acme.Services;
internal sealed class ChallengeService : IChallengeService {

    private readonly IAcmeClient _client;

    public IAccountData Account { get; }

    public Uri Location { get; }

    public string Type { get; }

    public string Token { get; }

    public string KeyAuthorization { get; }

    public ChallengeService(IAcmeClient client, IAccountData account, Uri location, string type, string token) {
        _client = client;
        Account = account;
        Location = location;
        Type = type;
        Token = token;
        KeyAuthorization = $"{Token}.{Base64Url.Encode(Account.KeyInfo.Thumbprint)}";
    }

    public ValueTask<AcmeResponse<AcmeChallenge>> Challenge()
        => _client.Post<AcmeChallenge, EmptyPayload>(Location, Account.Location, Account.KeyInfo, default);

    public ValueTask<AcmeResponse<AcmeChallenge>> Validate()
        => _client.Post<AcmeChallenge, EmptyObjectPayload>(Location, Account.Location, Account.KeyInfo, default);

}
