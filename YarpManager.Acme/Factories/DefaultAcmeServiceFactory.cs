using YarpManager.Acme.Services;

namespace YarpManager.Acme.Factories;
internal sealed class DefaultAcmeServiceFactory : IAcmeServiceFactory {

    private readonly IAcmeClientFactory _clientFactory;

    public DefaultAcmeServiceFactory(IAcmeClientFactory clientFactory) {
        _clientFactory = clientFactory;
    }

    public IAcmeService CreateService(Uri directoryUri) {

        var client = _clientFactory.CreateClient(directoryUri);

        return new AcmeService(client);
    }

    public IAcmeService CreateService(string directoryUri) {

        var client = _clientFactory.CreateClient(directoryUri);

        return new AcmeService(client);
    }

}
