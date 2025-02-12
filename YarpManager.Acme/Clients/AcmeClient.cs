using System.Buffers;
using System.Collections.Frozen;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using YarpManager.Acme.Jws;
using YarpManager.Acme.Jws.Jwk;
using YarpManager.Acme.Jws.Keys;
using YarpManager.Acme.Resources;
using YarpManager.Acme.Utils;
using YarpManager.Common.Result;

namespace YarpManager.Acme.Clients;
internal sealed class AcmeClient : IAcmeClient {

    private string? _nonce;
    private AcmeDirectory? _directory;

    private readonly Uri _directoryUri;
    private readonly HttpClient _httpClient;

    public AcmeClient(Uri directoryUri, HttpClient httpClient) {
        _directoryUri = directoryUri;
        _httpClient = httpClient;
    }

    public async ValueTask<Uri?> Resource(Expression<Func<AcmeDirectory, Uri?>> getResource, bool optional = false) {

        var directory = await GetDirectory();

        if (getResource.Body is not MemberExpression bodyMember)
            throw new ArgumentException("Expression should be a member");

        var resourceName = bodyMember.Member.Name;
        var uri = getResource.Compile()(directory);

        if (uri is null && !optional) {
            throw new ArgumentNullException(resourceName,
                $"{resourceName} is required.");
        }

        return uri;
    }

    public async ValueTask<AcmeDirectory> GetDirectory() {

        if (_directory is not null) return _directory;

        var response = await Get<AcmeDirectory>(_directoryUri);

        if (response.StatusCode is HttpStatusCode.OK) {
            _directory = response.Response.Value;
            _directory.Index = _directoryUri;

            _nonce = await GetNewNonce();

            return _directory;
        }

        throw response.Response.Throw();
    }

    public ValueTask<AcmeResponse<T>> Get<T>(Uri uri) {

        using var request = CreateMessage(HttpMethod.Get, uri);

        return SendAcmeRequest<T>(request);
    }

    public async ValueTask<AcmeResponse<T>> Post<T, TPayload>(Uri uri, Uri keyId, AsymmetricKey key, TPayload payload) {

        await GetDirectory(); // Ensure directory entity

        while (_nonce is null) { // Ensure nonce
            await GetNewNonce();
        }

        string json;
        if (key is RsaKey rsaKey) {

            var jwh = new JsonWebHeader<RsaJsonWebKey> {
                Algorithm = key.Algorithm,
                Nonce = _nonce,
                Url = uri,
                KeyId = keyId,
                JsonWebKey = new RsaJsonWebKey(rsaKey.Key)
            };

            var jwt = new JsonWebSignature<RsaJsonWebKey, TPayload> {
                Protected = jwh,
                Payload = payload
            };

            json = JsonSerializer.Serialize(jwt, JsonUtils.SerializerOptions);

        }
        else if (key is EcKey ecKey) {

            var jwh = new JsonWebHeader<EcJsonWebKey> {
                Algorithm = key.Algorithm,
                Nonce = _nonce,
                Url = uri,
                KeyId = keyId,
                JsonWebKey = new EcJsonWebKey(ecKey.Key)
            };

            var jwt = new JsonWebSignature<EcJsonWebKey, TPayload> {
                Protected = jwh,
                Payload = payload
            };

            json = JsonSerializer.Serialize(jwt, JsonUtils.SerializerOptions);
        }
        else {
            throw new NotSupportedException($"Key {key.GetType().Name} not supported.");
        }

        using var request = CreateMessage(HttpMethod.Post, uri);

        request.Content = new StringContent(json, Encoding.UTF8, AcmeConstants.JoseJsonMediaType);
        var response = await SendAcmeRequest<T>(request);

        

        return response;

    }

    public async ValueTask<AcmeResponse<T>> Post<T, TPayload>(Uri uri, AsymmetricKey key, TPayload payload) {

        await GetDirectory(); // Ensure directory entity

        while (_nonce is null) { // Ensure nonce
            await GetNewNonce();
        }

        string json;
        if (key is RsaKey rsaKey) {

            var jwh = new JsonWebHeader<RsaJsonWebKey> {
                Algorithm = key.Algorithm,
                Nonce = _nonce,
                Url = uri,
                JsonWebKey = new RsaJsonWebKey(rsaKey.Key)
            };

            var jwt = new JsonWebSignature<RsaJsonWebKey, TPayload> {
                Protected = jwh,
                Payload = payload
            };

            json = JsonSerializer.Serialize(jwt, JsonUtils.SerializerOptions);

        }
        else if (key is EcKey ecKey) {

            var jwh = new JsonWebHeader<EcJsonWebKey> {
                Algorithm = key.Algorithm,
                Nonce = _nonce,
                Url = uri,
                JsonWebKey = new EcJsonWebKey(ecKey.Key)
            };

            var jwt = new JsonWebSignature<EcJsonWebKey, TPayload> {
                Protected = jwh,
                Payload = payload
            };

            json = JsonSerializer.Serialize(jwt, JsonUtils.SerializerOptions);

        }
        else {
            throw new NotSupportedException($"Key {key.GetType().Name} not supported.");
        }

        _nonce = null;

        using var request = CreateMessage(HttpMethod.Post, uri);

        request.Content = new StringContent(json, Encoding.UTF8, AcmeConstants.JoseJsonMediaType);
        var response = await SendAcmeRequest<T>(request);
        
        return response;
    }

    private async ValueTask<AcmeResponse<T>> SendAcmeRequest<T>(HttpRequestMessage request) {

        using var response = await _httpClient.SendAsync(request);

        var nonce = await GetNonce(response);
        var result = await GetResult<T>(response);

        return new(
            response.StatusCode,
            response.Headers.Location,
            response.Content.Headers.ContentType?.MediaType,
            result,
            GetLinks(response),
            GetRetryAfter(response)
            );

    }

    private static async ValueTask<Result<T, AcmeError>> GetResult<T>(HttpResponseMessage response) {

        var result = Result.FromException<T, AcmeError>(new HttpRequestException($"Could not get {typeof(T).Name} resource."));

        if (IsJsonMediaType(response.Content.Headers.ContentType?.MediaType)) {

            if (response.IsSuccessStatusCode)
                result = (await response.Content.ReadFromJsonAsync<T>(JsonUtils.SerializerOptions))!;
            else
                result = (await response.Content.ReadFromJsonAsync<AcmeError>(JsonUtils.SerializerOptions))!;
        }
        else {

            try {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex) {
                result = Result.FromException<T, AcmeError>(ex);
            }
        }

        return result;
    }

    private ValueTask<string?> GetNonce(HttpResponseMessage response) {

        if (_nonce is not null) return ValueTask.FromResult<string?>(_nonce);

        if (response.Headers.TryGetValues("Replay-Nonce", out var values))
            return ValueTask.FromResult<string?>(_nonce = values.First());

        if (_directory is null) return ValueTask.FromResult<string?>(null);

        return GetNewNonce();
    }

    private async ValueTask<string?> GetNewNonce() {

        if (_nonce is not null) return _nonce;

        if (_directory is null) return null;

        using var request = CreateMessage(HttpMethod.Head, _directory.NewNonce);

        using var response = await _httpClient.SendAsync(request);

        if (!response.Headers.TryGetValues("Replay-Nonce", out var values))
            return null;

        return _nonce = values.First();
    }

    private static bool IsJsonMediaType(string? mediaType) {

        if (string.IsNullOrWhiteSpace(mediaType)) return false;

        var mediaTypeSpan = mediaType.AsSpan();
        if (mediaTypeSpan.StartsWith("application/")) {

            mediaTypeSpan = mediaTypeSpan[12..];
            var array = ArrayPool<Range>.Shared.Rent(mediaTypeSpan.Length);
            using var d0 = Deferer.Create(
                array => ArrayPool<Range>.Shared.Return(array), array);

            var itemsWritten = mediaTypeSpan.Split(array, '+');
            for (int i = 0; i < itemsWritten; ++i) {

                var range = array[i];
                var contains = mediaTypeSpan[range].Contains("json", StringComparison.OrdinalIgnoreCase);

                if (contains) return true;

            }

        }

        return false;
    }

    private static int GetRetryAfter(HttpResponseMessage response) {

        var retryAfter = response.Headers.RetryAfter;
        if (retryAfter is null) return 0;

        var date = retryAfter.Date;
        var delta = retryAfter.Delta;
        if (date.HasValue)
            return (int)Math.Abs((date.Value - DateTime.UtcNow).TotalSeconds);
        else if (delta.HasValue)
            return (int)delta.Value.TotalSeconds;

        return 0;
    }

    private FrozenDictionary<string, Uri[]> GetLinks(HttpResponseMessage response) {

        var headers = response.Headers;
        if (!headers.TryGetValues("Link", out var values)) return FrozenDictionary<string, Uri[]>.Empty;

        return values
            .Select(ParseLink)
            .ToLookup(ru => ru.Rel, ru => ru.Uri)
            .ToFrozenDictionary(g => g.Key, g => g.ToArray());
    }

    private (string Rel, Uri Uri) ParseLink(string input) {

        var inputSpan = input.AsSpan();

        var arrayToReturn = ArrayPool<Range>.Shared.Rent(2);
        using var d0 = Deferer.Create(
            array => ArrayPool<Range>.Shared.Return(array), arrayToReturn);

        inputSpan.Split(arrayToReturn, ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var ranges = arrayToReturn.AsSpan(0, 2);

        var uri = new Uri(inputSpan[ranges[0]][1..^1].ToString());
        var rel = inputSpan[ranges[1]]
            .TrimStart("rel=")
            .Trim('"')
            .ToString();

        return (rel, uri);
    }

    private HttpRequestMessage CreateMessage(HttpMethod method, Uri uri) {

        return new HttpRequestMessage {
            Method = method,
            RequestUri = uri,
            Version = _httpClient.DefaultRequestVersion,
            VersionPolicy = _httpClient.DefaultVersionPolicy,
        };

    }


}
