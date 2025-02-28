using System.Linq.Expressions;
using YarpManager.Acme.Jws.Keys;
using YarpManager.Acme.Resources;

namespace YarpManager.Acme.Clients;
public interface IAcmeClient {

    ValueTask<AcmeResponse<Uri?>> Resource(Expression<Func<AcmeDirectory, Uri?>> getResource, bool optional = false);

    ValueTask<AcmeResponse<AcmeDirectory>> GetDirectory();

    ValueTask<AcmeResponse<T>> Get<T>(Uri uri);

    ValueTask<AcmeResponse<T>> Post<T, TPayload>(Uri uri, AsymmetricKeyInfo key, TPayload payload);

    ValueTask<AcmeResponse<T>> Post<T, TPayload>(Uri uri, Uri keyId, AsymmetricKeyInfo key, TPayload payload);

}
