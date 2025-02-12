using System.Linq.Expressions;
using YarpManager.Acme.Jws.Keys;
using YarpManager.Acme.Resources;

namespace YarpManager.Acme.Clients;
public interface IAcmeClient {

    ValueTask<Uri?> Resource(Expression<Func<AcmeDirectory, Uri?>> getResource, bool optional = false);

    ValueTask<AcmeDirectory> GetDirectory();

    ValueTask<AcmeResponse<T>> Get<T>(Uri uri);

    ValueTask<AcmeResponse<T>> Post<T, TPayload>(Uri uri, AsymmetricKey key, TPayload payload);

    ValueTask<AcmeResponse<T>> Post<T, TPayload>(Uri uri, Uri keyId, AsymmetricKey key, TPayload payload);

}
