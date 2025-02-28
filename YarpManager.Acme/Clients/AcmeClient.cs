using System;
using System.Buffers;
using System.Collections.Frozen;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using YarpManager.Acme.Jws;
using YarpManager.Acme.Jws.Jwk;
using YarpManager.Acme.Jws.Keys;
using YarpManager.Acme.Resources;
using YarpManager.Acme.Utils;
using YarpManager.Common;
using YarpManager.Common.Result;

namespace YarpManager.Acme.Clients;
internal sealed class AcmeClient : IAcmeClient {

    private readonly HttpClient _httpClient;

    private Uri _directoryUri;
    private string? _nonce;

    public AcmeClient(Uri directoryUri, HttpClient httpClient) {
        _directoryUri = directoryUri;
        _httpClient = httpClient;
    }

    public async ValueTask<AcmeResponse<Uri?>> Resource(Expression<Func<AcmeDirectory, Uri?>> getResource, bool optional = false) {

        var dirRes = await GetDirectory();

        if (!dirRes.TryGet(out var dir)) return dirRes.To<Uri?>();

        if (getResource.Body is not MemberExpression bodyMember)
            throw new ArgumentException("Expression should be a member");

        var resourceName = bodyMember.Member.Name;

        var uri = getResource.Compile()(dir!);

        if (uri is null && !optional) {

            var error = new AcmeError {
                Type = AcmeErrorType.ClientInternal,
                Detail = $"{resourceName} is required."
            };

            return new(
                HttpStatusCode.InternalServerError,
                null,
                null,
                error,
                FrozenDictionary<string, Uri[]>.Empty,
                0);
        }

        return dirRes.To(uri);
    }

    public async ValueTask<AcmeResponse<AcmeDirectory>> GetDirectory() {

        var response = await Get<AcmeDirectory>(_directoryUri);

        return response.To(res => res.Content.Value);
    }

    public ValueTask<AcmeResponse<T>> Get<T>(Uri uri) {

        using var request = CreateMessage(HttpMethod.Get, uri);

        return SendGetAcmeRequest<T>(request);
    }

    public async ValueTask<AcmeResponse<T>> Post<T, TPayload>(Uri uri, Uri keyId, AsymmetricKeyInfo key, TPayload payload) {

        int retryCount = 0;
        AcmeResponse<T> response;
        do {

            var jwsRes = await GenerateJws(uri, keyId, key, payload);

            if (!jwsRes.TryGet(out var jws))
                return jwsRes.To<T>();

            Debug.Assert(jws is not null);

            using var request = CreateMessage(HttpMethod.Post, uri);

            request.Content = new StringContent(jws, Encoding.UTF8, AcmeConstants.JoseJsonMediaType);
            response = await SendPostAcmeRequest<T>(request);

        } while (response.StatusCode is HttpStatusCode.BadRequest &&
            response.Content.Error.Type is AcmeErrorType.BadNonce &&
            retryCount++ < 3);

        return response;
    }

    public ValueTask<AcmeResponse<T>> Post<T, TPayload>(Uri uri, AsymmetricKeyInfo key, TPayload payload)
        => Post<T, TPayload>(uri, null!, key, payload);

    private async ValueTask<AcmeResponse<string>> GenerateJws<TPayload>(Uri url, Uri? keyId, AsymmetricKeyInfo key, TPayload payload) {

        string? nonce;
        if (_nonce is not null) nonce = _nonce;
        else {

            var nonceRes = await FetchNewNonce();

            if (!nonceRes.TryGet(out nonce))
                return nonceRes.To<string>();

        }

        Debug.Assert(nonce is not null);

        string json;
        if (key is RsaKeyInfo rsaKey) {

            var jwh = new JsonWebHeader<RsaJsonWebKey> {
                Algorithm = key.Algorithm,
                Nonce = nonce,
                Url = url,
                KeyId = keyId,
                JsonWebKey = rsaKey.JsonWebKey
            };

            var jwt = new JsonWebSignature<RsaJsonWebKey, TPayload> {
                Protected = jwh,
                Payload = payload
            };

            json = JsonSerializer.Serialize(jwt, JsonUtils.SerializerOptions);

        }
        else if (key is EcKeyInfo ecKey) {

            var jwh = new JsonWebHeader<EcJsonWebKey> {
                Algorithm = key.Algorithm,
                Nonce = nonce,
                Url = url,
                KeyId = keyId,
                JsonWebKey = ecKey.JsonWebKey
            };

            var jwt = new JsonWebSignature<EcJsonWebKey, TPayload> {
                Protected = jwh,
                Payload = payload
            };

            json = JsonSerializer.Serialize(jwt, JsonUtils.SerializerOptions);

        }
        else {

            var error = new AcmeError {
                Type = AcmeErrorType.ClientInternal,
                Detail = $"Key {key.GetType().Name} not supported."
            };

            return AcmeResponse<string>.From(error);
        }

        return AcmeResponse<string>.From(json);
    }

    private async ValueTask<AcmeResponse<T>> SendGetAcmeRequest<T>(HttpRequestMessage request) {

        try {

            using var response = await _httpClient.SendAsync(request);

            var result = await GetResult<T>(response);

            return new(
                response.StatusCode,
                response.Headers.Location,
                response.Content.Headers.ContentType?.MediaType,
                result,
                GetLinks(response),
                GetRetryAfter(response));

        }
        catch (Exception ex) {
            return AcmeResponse<T>.From(ex);
        }



    }

    private async ValueTask<AcmeResponse<T>> SendPostAcmeRequest<T>(HttpRequestMessage request) {

        _nonce = null;
        try {

            using var response = await _httpClient.SendAsync(request);

            CacheNonce(response);
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
        catch (Exception ex) {
            return AcmeResponse<T>.From(ex);
        }

    }

    private static async ValueTask<Result<T, AcmeError>> GetResult<T>(HttpResponseMessage response) {

        try {

            var mediaType = response.Content.Headers.ContentType?.MediaType;

            if (IsJsonMediaType(mediaType)) {

#if DEBUG
                var json = await response.Content.ReadAsStringAsync();
#endif

                if (response.IsSuccessStatusCode)
                    return (await response.Content.ReadFromJsonAsync<T>(JsonUtils.SerializerOptions))!;
                else
                    return (await response.Content.ReadFromJsonAsync<AcmeError>(JsonUtils.SerializerOptions))!;
            }
            else if (typeof(T) == typeof(string)) {
                var str = await response.Content.ReadAsStringAsync();
                return (T)(object)str;
            }
            else if (typeof(T) == typeof(byte[])) {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                return (T)(object)bytes;
            }
            else if (typeof(T) == typeof(X509Certificate2)) {

                var cert = await GetCert(mediaType, response.Content);

                if (cert is null) {
                    return new AcmeError {
                        Type = AcmeErrorType.ClientInternal,
                        Detail = "Could not get certificate."
                    };
                }

                return (T)(object)cert;

            }
            else {
                response.EnsureSuccessStatusCode();
            }

        }
        catch (Exception ex) {
            return Result.FromException<T, AcmeError>(ex);
        }

        return Result.FromException<T, AcmeError>(new HttpRequestException($"Could not get {typeof(T).Name} resource."));
    }

    private void CacheNonce(HttpResponseMessage response) {

        if (_nonce is not null) return;

        if (response.Headers.TryGetValues("Replay-Nonce", out var values))
            _nonce = values.First();

    }

    private async ValueTask<AcmeResponse<string>> FetchNewNonce() {

        var resouceRes = await Resource(dir => dir.NewNonce);

        if (!resouceRes.TryGet(out var newNonceUri))
            return resouceRes.To<string>();

        using var request = CreateMessage(HttpMethod.Head, newNonceUri!);

        using var response = await _httpClient.SendAsync(request);

        if (response.Headers.TryGetValues("Replay-Nonce", out var values))
            return resouceRes.To(values.First());

        var error = new AcmeError {
            Type = AcmeErrorType.ClientInternal,
            Detail = "Could not get nonce."
        };

        return AcmeResponse<string>.From(error);
    }

    private static async ValueTask<X509Certificate2?> GetCert(string? mediaType, HttpContent content) {

        if (string.IsNullOrWhiteSpace(mediaType)) return null;

        return mediaType switch {
            "application/pkix-cert" => new X509Certificate2(await content.ReadAsByteArrayAsync()),
            "application/pem-certificate-chain" => X509Certificate2.CreateFromPem(await content.ReadAsStringAsync()),
            "application/pkcs7-mime" => new X509Certificate2(await content.ReadAsByteArrayAsync()),
            _ => null,
        };

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

    // TODO: Maybe instead of int TimeSpan
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

        var links = values
            .Select(ParseLink)
            .ToLookup(ru => ru.Rel, ru => ru.Uri, StringComparer.OrdinalIgnoreCase)
            .ToFrozenDictionary(g => g.Key, g => g.ToArray(), StringComparer.OrdinalIgnoreCase);

        if (links.TryGetValue("index", out var uris))
            _directoryUri = uris[0];

        return links;
    }

    private (string Rel, Uri Uri) ParseLink(string input) {

        var inputSpan = input.AsSpan();

        using var ranges = new PooledArray<Range>(2);

        inputSpan.Split(ranges, ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

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
