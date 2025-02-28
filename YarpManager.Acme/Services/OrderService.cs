using System.Diagnostics;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;
using YarpManager.Acme.Abstractions;
using YarpManager.Acme.Clients;
using YarpManager.Acme.Jws;
using YarpManager.Acme.Resources;
using YarpManager.Acme.Utils;

namespace YarpManager.Acme.Services;
internal sealed class OrderService : IOrderService {

    private readonly IAcmeClient _client;

    public IAccountData Account { get; }

    public Uri Location { get; }

    public OrderService(IAcmeClient client, IAccountData account, Uri location) {
        _client = client;
        Account = account;
        Location = location;

    }

    public ValueTask<AcmeResponse<AcmeOrder>> Order()
        => _client.Post<AcmeOrder, EmptyPayload>(
                Location, Account.Location, Account.KeyInfo, default);

    public async ValueTask<AcmeResponse<IEnumerable<IAuthorizationService>>> Authorizations() {

        var authz = await Order().Get(o => o.Authorizations);

        if (!authz.TryGet(out var authzUris))
            return authz.To<IEnumerable<IAuthorizationService>>();

        return authz.To(GetAuthorizationServiceEnumerable, this);

    }
    private static IEnumerable<IAuthorizationService> GetAuthorizationServiceEnumerable(AcmeResponse<Uri[]> res, OrderService orderService) {

        var uris = res.Content.Value;

        foreach (var uri in uris)
            yield return new AuthorizationService(orderService._client, orderService.Account, uri);

    }

    public async ValueTask<AcmeResponse<AcmeOrder>> Finalize(CsrInfo csrInfo) {

        var orderRes = await Order();

        if (!orderRes.TryGet(out var order))
            return orderRes.To<AcmeOrder>();

        SubjectAlternativeNameBuilder alternativeSubjectBuilder = new();
        string? commonName = null;
        foreach (var identifiers in order.Identifiers) {

            if (identifiers.Type is not AcmeIdentifierType.Dns) continue;

            commonName ??= identifiers.Value;

            alternativeSubjectBuilder.AddDnsName(identifiers.Value);

        }

        Debug.Assert(commonName is not null);

        X500DistinguishedNameBuilder subjectNameBuilder = new();

        subjectNameBuilder.AddCommonName(commonName);

        if (csrInfo.OrganizationName is not null)
            subjectNameBuilder.AddOrganizationName(csrInfo.OrganizationName);

        if (csrInfo.OrganizationUnitName is not null)
            subjectNameBuilder.AddOrganizationalUnitName(csrInfo.OrganizationUnitName);

        if (csrInfo.LocalityName is not null)
            subjectNameBuilder.AddLocalityName(csrInfo.LocalityName);

        if (csrInfo.StateOrProvinceName is not null)
            subjectNameBuilder.AddStateOrProvinceName(csrInfo.StateOrProvinceName);

        if (csrInfo.CountryOrRegion is not null)
            subjectNameBuilder.AddCountryOrRegion(csrInfo.CountryOrRegion);

        var csr = Account.KeyInfo.CreateEmptyCertificateRequest(subjectNameBuilder.Build());

        csr.CertificateExtensions.Add(alternativeSubjectBuilder.Build());

        return await _client.Post<AcmeOrder, AcmeOrder.FinalizeRequest>(
            order.Finalize, Account.Location, Account.KeyInfo,
            new AcmeOrder.FinalizeRequest {
                Csr = csr.CreateSigningRequest()
            }
        );

    }

    public async ValueTask<AcmeResponse<X509Certificate2>> Download() {

        var certificateUriRes = await Order().Get(o => o.Certificate);

        if (!certificateUriRes.TryGet(out var certificateUri))
            return certificateUriRes.To<X509Certificate2>();

        if (certificateUri is null) {

            var error = new AcmeError {
                Type = AcmeErrorType.ClientInternal,
                Detail = "Certificate not found"
            };

            return AcmeResponse<X509Certificate2>.From(error);
        }

        //ArrayBufferWriter<byte> buffer = new();

        var chainRes = await _client.Post<byte[], EmptyPayload>(
                                certificateUri, Account.Location, Account.KeyInfo, default);

        
        return chainRes.To(res => new X509Certificate2(res.Content.Value.AsSpan(), string.Empty, X509KeyStorageFlags.Exportable));
    }


}
