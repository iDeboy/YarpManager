using YarpManager.Acme.Jws;
using YarpManager.Acme.Jws.Keys;
using YarpManager.Acme.Resources;

namespace YarpManager.Acme.Services;
public interface IAcmeService {


    ValueTask<AcmeResponse<AcmeDirectory>> GetDirectory();

    ValueTask<AcmeResponse<IAccountService>> Account(AsymmetricKeyInfo key);

    ValueTask<AcmeResponse<IAccountService>> NewAccount(string[] contact, bool termsOfServiceAgreed, JsonSignAlgorithm keyAlgorithm = JsonSignAlgorithm.RS256);

    //ValueTask<AcmeResponse<T>> Get<T>(Uri uri);

    //ValueTask<AcmeResponse<T>> Post<T, TPayload>(Uri uri, AsymmetricAlgorithm key, TPayload payload);

}
