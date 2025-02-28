using YarpManager.Acme.Resources;

namespace YarpManager.Acme.Utils;
public static class AcmeResponseExtensions {

    public static async ValueTask<AcmeResponse<U>> Get<T, U>(this ValueTask<AcmeResponse<T>> responseTask, Func<T, U> valueFn) {

        var res = await responseTask;

        if (res.TryGet(out var value))
            return AcmeResponse<U>.From(valueFn(value));

        return res.To<U>();
    }

}
