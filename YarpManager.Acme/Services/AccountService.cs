using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using YarpManager.Acme.Abstractions;
using YarpManager.Acme.Clients;
using YarpManager.Acme.Jws;
using YarpManager.Acme.Jws.Jwk;
using YarpManager.Acme.Jws.Keys;
using YarpManager.Acme.Resources;
using YarpManager.Acme.Utils;

namespace YarpManager.Acme.Services;
internal sealed class AccountService : IAccountService {

    private readonly IAcmeClient _client;

    public Uri Location { get; }

    public AsymmetricKeyInfo KeyInfo { get; private set; }

    public AccountService(IAcmeClient client, Uri location, AsymmetricKeyInfo key) {
        _client = client;
        Location = location;
        KeyInfo = key;
    }

    public ValueTask<AcmeResponse<AcmeAccount>> Account()
        => _client.Post<AcmeAccount, EmptyPayload>(
                Location, Location, KeyInfo, default);

    public async ValueTask<AcmeResponse<AcmeAccount>> ChangeKey(AsymmetricKeyInfo key) {

        var keyChangeRes = await _client.Resource(d => d.KeyChange);

        if (!keyChangeRes.TryGet(out var keyChangeUri))
            return keyChangeRes.To<AcmeAccount>();

        var json = (key, KeyInfo) switch {

            (RsaKeyInfo newKey, RsaKeyInfo oldKey) => JsonSerializer.Serialize(
                new JsonWebSignature<RsaJsonWebKey, AcmeKeyChange<RsaJsonWebKey>> {
                    Protected = new() {
                        Algorithm = newKey.Algorithm,
                        JsonWebKey = newKey.JsonWebKey,
                        Url = keyChangeUri,
                    },
                    Payload = new() {
                        Account = Location,
                        OldKey = oldKey.JsonWebKey,
                    }
                },
                JsonUtils.SerializerOptions),

            (RsaKeyInfo newKey, EcKeyInfo oldKey) => JsonSerializer.Serialize(
                new JsonWebSignature<RsaJsonWebKey, AcmeKeyChange<EcJsonWebKey>> {
                    Protected = new() {
                        Algorithm = newKey.Algorithm,
                        JsonWebKey = newKey.JsonWebKey,
                        Url = keyChangeUri,
                    },
                    Payload = new() {
                        Account = Location,
                        OldKey = oldKey.JsonWebKey,
                    }
                },
                JsonUtils.SerializerOptions),

            (EcKeyInfo newKey, RsaKeyInfo oldKey) => JsonSerializer.Serialize(
                new JsonWebSignature<EcJsonWebKey, AcmeKeyChange<RsaJsonWebKey>> {
                    Protected = new() {
                        Algorithm = newKey.Algorithm,
                        JsonWebKey = newKey.JsonWebKey,
                        Url = keyChangeUri,
                    },
                    Payload = new() {
                        Account = Location,
                        OldKey = oldKey.JsonWebKey,
                    }
                },
                JsonUtils.SerializerOptions),

            (EcKeyInfo newKey, EcKeyInfo oldKey) => JsonSerializer.Serialize(
                new JsonWebSignature<EcJsonWebKey, AcmeKeyChange<EcJsonWebKey>> {
                    Protected = new() {
                        Algorithm = newKey.Algorithm,
                        JsonWebKey = newKey.JsonWebKey,
                        Url = keyChangeUri,
                    },
                    Payload = new() {
                        Account = Location,
                        OldKey = oldKey.JsonWebKey,
                    }
                },
                JsonUtils.SerializerOptions),

            _ => throw new NotSupportedException("Unsuported key."),
        };

        var response = await _client.Post<AcmeAccount, string>(keyChangeUri, Location, KeyInfo, json);

        if (response.IsSuccessStatusCode) {
            KeyInfo.Dispose();
            KeyInfo = key;
        }

        return response;
    }

    public ValueTask<AcmeResponse<AcmeAccount>> Deactivate() {

        return _client.Post<AcmeAccount, AcmeAccount>(Location,
            Location,
            KeyInfo,
            new() { Status = AccountStatusType.Deactivated });

    }

    public async ValueTask<AcmeResponse<IOrderService>> NewOrder(string[] domains, DateTimeOffset? notBefore = null, DateTimeOffset? notAfter = null) {

        var resourceRes = await _client.Resource(dir => dir.NewOrder);

        if (!resourceRes.TryGet(out var newOrderUri))
            return resourceRes.To<IOrderService>();

        var order = new AcmeOrder.Request {
            Identifiers = domains.Select(domain => new AcmeIdentifier {
                Type = AcmeIdentifierType.Dns,
                Value = domain
            }).ToArray(),
            NotBefore = notBefore,
            NotAfter = notAfter,
        };

        var response = await _client.Post<AcmeOrder, AcmeOrder.Request>(newOrderUri, Location, KeyInfo, order);

        return response.To<IOrderService, AccountService>(
            (res, service) => new OrderService(service._client, service, res.Location!),
            this);
    }

    public void Dispose() {
        KeyInfo.Dispose();
    }

    public bool SaveKey(string path)
        => KeyInfo.SaveToFile(path);

}
