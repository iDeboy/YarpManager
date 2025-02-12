using System.Net.Http.Headers;
using System.Net.Mime;

namespace YarpManager.Acme.Utils;
internal static class AcmeConstants {

    public const string MimeJoseJson = "application/jose+json";

    public const string HttpClientName = "acme";

    public const string TestDirectory = "https://acme-staging-v02.api.letsencrypt.org/directory";

    public static readonly MediaTypeHeaderValue JoseJsonMediaType = new(MimeJoseJson, null);

}
