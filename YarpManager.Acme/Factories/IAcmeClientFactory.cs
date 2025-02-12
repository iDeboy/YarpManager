using YarpManager.Acme.Clients;
using YarpManager.Acme.Services;

namespace YarpManager.Acme.Factories;
public interface IAcmeClientFactory {

    IAcmeClient CreateClient(Uri directoryUri);

    IAcmeClient CreateClient(string directoryUri);

}
