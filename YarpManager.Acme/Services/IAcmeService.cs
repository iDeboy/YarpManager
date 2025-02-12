using YarpManager.Acme.Jws;
using YarpManager.Acme.Jws.Keys;
using YarpManager.Acme.Resources;

namespace YarpManager.Acme.Services;
public interface IAcmeService {


    ValueTask<AcmeDirectory> GetDirectory();

    ValueTask<IAccountService> Account(AsymmetricKey key);

    ValueTask<IAccountService> NewAccount(string[] contact, bool termsOfServiceAgreed, JsonSignAlgorithm keyAlgorithm = JsonSignAlgorithm.RS256);

    //ValueTask<AcmeResponse<T>> Get<T>(Uri uri);

    //ValueTask<AcmeResponse<T>> Post<T, TPayload>(Uri uri, AsymmetricAlgorithm key, TPayload payload);

}
