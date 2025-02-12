using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;

namespace YarpManager.Acme.Handlers;
internal sealed class AcmeHandler : DelegatingHandler {

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {

        bool consumedNonce = false;
        string? nonce = null;

        var headers = request.Headers;

        if (headers.TryGetValues("Replay-Nonce", out var values)) {

            nonce = values.Single();

            headers.Remove("Replay-Nonce");
        }

        if (headers.Contains("X-Sign-Jws")) {

            if (!headers.TryGetValues("X-Private-Key", out var privateKeyValues))
                throw new InvalidOperationException("Private key was not provided.");

            var privateKey = privateKeyValues.Single();

            Debug.Assert(nonce is not null);
            Debug.Assert(request.Content is not null);
            Debug.Assert(request.RequestUri is not null);

            var payload = await request.Content.ReadAsStringAsync(cancellationToken);

            var jws = GenerateJws(payload, nonce, null!/*privateKey*/, null!/*kid*/, request.RequestUri);

            headers.Remove("X-Private-Key");
            headers.Remove("X-Sign-Jws");
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (consumedNonce)
            response.Headers.TryAddWithoutValidation("Consumed-Nonce", "true");


        return response;
    }

    private string GenerateJws(string payload, string nonce, ECDsa privateKey, string kid, Uri url) {
        return string.Empty;
        // Crear el encabezado del JWS
        //var header = new Dictionary<string, object>
        //{
        //    { "alg", "ES256" },
        //    { "kid", kid },
        //    { "nonce", nonce },
        //    { "url", url.AbsoluteUri }
        //};
        //// TODO: Crear propias clases
        //// Crear el descriptor del token
        //var tokenDescriptor = new SecurityTokenDescriptor {
        //    Claims = header,
        //    Subject = new ClaimsIdentity(),
        //    IssuedAt = DateTime.UtcNow,
        //    SigningCredentials = new SigningCredentials(
        //        new ECDsaSecurityKey(privateKey),
        //        SecurityAlgorithms.EcdsaSha256)

        //};

        //// Incluir el payload como "aud"
        //tokenDescriptor.AdditionalHeaderClaims["payload"] = payload;

        //// Generar el token JWS
        //var tokenHandler = new JsonWebTokenHandler();
        //return tokenHandler.CreateToken(tokenDescriptor);
    }

}
