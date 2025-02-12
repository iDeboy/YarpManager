using System.Net;
using YarpManager.Common.Result;

namespace YarpManager.Acme.Resources;
public record AcmeResponse<T>(HttpStatusCode StatusCode, Uri? Location, string? ContentType, Result<T, AcmeError> Response, IDictionary<string, Uri[]> Links, int RetryAfter);
