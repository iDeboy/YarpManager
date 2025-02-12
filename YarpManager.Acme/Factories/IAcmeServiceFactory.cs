using YarpManager.Acme.Jws;
using YarpManager.Acme.Jws.Keys;
using YarpManager.Acme.Services;

namespace YarpManager.Acme.Factories;
public interface IAcmeServiceFactory {

    IAcmeService CreateService(Uri directoryUri);
    IAcmeService CreateService(string directoryUri);

}
