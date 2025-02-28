using System.Buffers;
using System.Diagnostics;
using System.Net;
using YarpManager.Acme.Clients;
using YarpManager.Acme.Jws;
using YarpManager.Acme.Jws.Keys;
using YarpManager.Acme.Resources;
using YarpManager.Common;

namespace YarpManager.Acme.Services;

// TODO:
//  - This class should be on a middleware, to select the ACME Server for a specified certificate if exists
//  - Implement AcmeResponse<T> to save StatusCode, Content-Type, Response, Errors, Links, Location and RetryAfter 

internal sealed class AcmeService : IAcmeService {

    private readonly IAcmeClient _client;

    public AcmeService(IAcmeClient client) {
        _client = client;
    }

    public ValueTask<AcmeResponse<AcmeDirectory>> GetDirectory()
       => _client.GetDirectory();

    public async ValueTask<AcmeResponse<IAccountService>> Account(AsymmetricKeyInfo key) {

        var resourceRes = await _client.Resource(d => d.NewAccount);

        if (!resourceRes.TryGet(out var newAccountUri))
            return resourceRes.To<IAccountService>();

        Debug.Assert(newAccountUri is not null);

        var account = new AcmeAccount.Request {
            OnlyReturnExisting = true,
        };

        var response = await _client.Post<AcmeAccount, AcmeAccount.Request>(
            newAccountUri,
            key,
            account);

        if (!response.IsSuccessStatusCode) key.Dispose();

        return response.To<IAccountService, (IAcmeClient client, AsymmetricKeyInfo key)>((res, args) => new AccountService(args.client, res.Location!, args.key), (_client, key));
    }
   
    public async ValueTask<AcmeResponse<IAccountService>> NewAccount(string[] contact, bool termsOfServiceAgreed, JsonSignAlgorithm keyAlgorithm = JsonSignAlgorithm.RS256) {

        var resourceRes = await _client.Resource(d => d.NewAccount);

        if (!resourceRes.TryGet(out var newAccountUri))
            return resourceRes.To<IAccountService>();

        Debug.Assert(newAccountUri is not null);

        contact = contact
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(FormatContact)
            .ToArray();

        var account = new AcmeAccount.Request {
            Contact = contact,
            TermsOfServiceAgreed = termsOfServiceAgreed
        };

        AcmeResponse<AcmeAccount>? response;
        AsymmetricKeyInfo key;
        do {

            key = AsymmetricKeyInfo.Create(keyAlgorithm);

            response = await _client.Post<AcmeAccount, AcmeAccount.Request>(
            newAccountUri,
            key,
            account);

            if (!response.IsSuccessStatusCode) key.Dispose();

            --keyAlgorithm;

        } while (response.StatusCode is HttpStatusCode.BadRequest &&
            response.Content.Error.Type is AcmeErrorType.Malformed or AcmeErrorType.BadPublicKey);

        return response.To<IAccountService, (IAcmeClient client, AsymmetricKeyInfo key)>((res, args) => new AccountService(args.client, res.Location!, args.key), (_client, key));
    }

    private static string FormatContact(string input) {

        ReadOnlySpan<char> Mailto = "mailto:";
        const int MailtoLength = 7;

        using var contact = new PooledArray<char>(MailtoLength + input.Length);

        var span = input.AsSpan();
        int outputLen;

        if (!span.StartsWith(Mailto)) {

            Mailto.CopyTo(contact);

            outputLen = span
                .ToLowerInvariant(contact.AsSpan(MailtoLength));

            outputLen += MailtoLength;

        }
        else {
            outputLen = span.ToLowerInvariant(contact);
        }


        return contact.AsSpan(0, outputLen).ToString();

    }

}
