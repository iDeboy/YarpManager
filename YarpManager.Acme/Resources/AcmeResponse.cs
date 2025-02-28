using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using YarpManager.Common;
using YarpManager.Common.Result;

namespace YarpManager.Acme.Resources;
public record AcmeResponse<T>(HttpStatusCode StatusCode,
    Uri? Location,
    string? ContentType,
    Result<T, AcmeError> Content,
    IDictionary<string, Uri[]> Links,
    // TODO: Maybe instead on int TimeSpan
    int RetryAfter) {

    public bool IsSuccessStatusCode => ((int)StatusCode >= 200) && ((int)StatusCode <= 299);

    public bool TryGet([NotNullWhen(true)] out T? resouce) {

        if (!IsSuccessStatusCode) {
            resouce = default;
            return false;
        }

        return Content.TryGet(out resouce);
    }

    public AcmeResponse<U> To<U>() {

        if (Content.IsError)
            return new(StatusCode, Location, ContentType, Content.Error, Links, RetryAfter);

        if (Content.IsException)
            return new(StatusCode, Location, ContentType,
                Result.FromException<U, AcmeError>(Content.Exception),
                Links, RetryAfter);

        var error = new AcmeError {
            Type = AcmeErrorType.ClientInternal,
            Detail = "Could not transform response.",
        };

        return new(StatusCode, Location, ContentType, error, Links, RetryAfter);
    }

    public static AcmeResponse<T> From(T value) {

        return new(HttpStatusCode.OK,
            null,
            null,
            value,
            FrozenDictionary<string, Uri[]>.Empty,
            0);

    }

    public static AcmeResponse<T> From(AcmeError error) {

        return new(HttpStatusCode.InternalServerError,
            null,
            null,
            error,
            FrozenDictionary<string, Uri[]>.Empty,
            0);

    }

    public static AcmeResponse<T> From(Exception ex) {

        using var subproblems = new PooledArray<AcmeError.SubProblem>(100);
        int i = 0;
        var current = ex.InnerException;
        while (current is not null) {
            subproblems[i++] = new() {
                Type = AcmeErrorType.ClientInternal,
                Detail = current.Message,
                Identifier = new() {
                    Type = current.GetType().Name,
                    Value = current.Source ?? string.Empty,
                }
            };
            current = current.InnerException;
        }

        var error = new AcmeError {
            Type = AcmeErrorType.ClientInternal,
            Detail = ex.Message,
            Subproblems = subproblems.AsSpan(0, i).ToArray()
        };

        return From(error);

    }

    public AcmeResponse<U> To<U>(U value) {

        if (Content.IsError)
            return new(StatusCode, Location, ContentType, Content.Error, Links, RetryAfter);

        if (Content.IsException)
            return new(StatusCode, Location, ContentType,
                Result.FromException<U, AcmeError>(Content.Exception),
                Links, RetryAfter);

        return new(StatusCode, Location, ContentType, value, Links, RetryAfter);
    }

    public AcmeResponse<U> To<U>(Func<AcmeResponse<T>, U> valueFn) {

        if (Content.IsError)
            return new(StatusCode, Location, ContentType, Content.Error, Links, RetryAfter);

        if (Content.IsException)
            return new(StatusCode, Location, ContentType,
                Result.FromException<U, AcmeError>(Content.Exception),
                Links, RetryAfter);

        return new(StatusCode, Location, ContentType, valueFn(this), Links, RetryAfter);
    }

    public AcmeResponse<U> To<U, TModel>(Func<AcmeResponse<T>, TModel, U> valueFn, TModel model) {

        if (Content.IsError)
            return new(StatusCode, Location, ContentType, Content.Error, Links, RetryAfter);

        if (Content.IsException)
            return new(StatusCode, Location, ContentType,
                Result.FromException<U, AcmeError>(Content.Exception),
                Links, RetryAfter);

        return new(StatusCode, Location, ContentType, valueFn(this, model), Links, RetryAfter);
    }

}
