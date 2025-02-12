using YarpManager.Acme.Clients;
using YarpManager.Acme.Utils;

namespace YarpManager.Acme.Factories;

internal sealed class DefaultAcmeClientFactory : IAcmeClientFactory {

    private readonly IHttpClientFactory _httpClientFactory;

    public DefaultAcmeClientFactory(IHttpClientFactory httpClientFactory) {
        _httpClientFactory = httpClientFactory;
    }

    public IAcmeClient CreateClient(Uri directoryUri) {

        var http = _httpClientFactory.CreateClient(AcmeConstants.HttpClientName);

        return new AcmeClient(directoryUri, http);
    }

    public IAcmeClient CreateClient(string directoryUri)
        => CreateClient(new Uri(directoryUri));

}
