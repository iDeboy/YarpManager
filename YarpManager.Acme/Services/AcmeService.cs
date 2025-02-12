using System.Buffers;
using System.Diagnostics;
using YarpManager.Acme.Clients;
using YarpManager.Acme.Jws;
using YarpManager.Acme.Jws.Keys;
using YarpManager.Acme.Resources;
using YarpManager.Acme.Utils;

namespace YarpManager.Acme.Services;

// TODO:
//  - This class should be on a middleware, to select the ACME Server for a specified certificate if exists
//  - Implement AcmeResponse<T> to save StatusCode, Content-Type, Response, Errors, Links, Location and RetryAfter 

internal sealed class AcmeService : IAcmeService {

    private readonly IAcmeClient _client;

    public AcmeService(IAcmeClient client) {
        _client = client;
    }

    public ValueTask<IAccountService> Account(AsymmetricKey key) {
        throw new NotImplementedException();
    }

    public ValueTask<AcmeDirectory> GetDirectory()
        => _client.GetDirectory();

    public async ValueTask<IAccountService> NewAccount(string[] contact, bool termsOfServiceAgreed, JsonSignAlgorithm keyAlgorithm = JsonSignAlgorithm.RS256) {

        var newAccountUri = await _client.Resource(d => d.NewAccount);

        Debug.Assert(newAccountUri is not null);

        contact = contact
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(FormatContact)
            .ToArray();

        var account = new AcmeAccount.Request {
            Contact = contact,
            TermsOfServiceAgreed = termsOfServiceAgreed
        };

        var key = AsymmetricKey.Create(keyAlgorithm);

        var response = await _client.Post<AcmeAccount, AcmeAccount.Request>(
            newAccountUri,
            key,
            account);

        return new AccountService(response.Location!, key);
    }

    private static string FormatContact(string input) {

        ReadOnlySpan<char> Mailto = "mailto:";
        const int MailtoLength = 7;

        var array = ArrayPool<char>.Shared.Rent(MailtoLength + input.Length);
        using var d0 = Deferer.Create(
            array => ArrayPool<char>.Shared.Return(array), array);

        var span = input.AsSpan();
        int outputLen;

        if (!span.StartsWith(Mailto)) {

            Mailto.CopyTo(array);

            outputLen = span
                .ToLowerInvariant(array.AsSpan(MailtoLength));

            outputLen += MailtoLength;

        }
        else {
            outputLen = span.ToLowerInvariant(array);
        }


        return array.AsSpan(0, outputLen).ToString();

    }

}
