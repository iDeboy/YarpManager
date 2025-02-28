using System.Buffers;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using YarpManager.Acme.Jws.Jwk;
using YarpManager.Acme.Utils;
using YarpManager.Common;

namespace YarpManager.Acme.Jws;
public static class JwsSigner {

    public static string Sign<TJwk>(
        JsonWebHeader<TJwk> header,
        ReadOnlySpan<char> encodedHeader,
        ReadOnlySpan<char> encodedPayload) where TJwk : JsonWebKey {

        var signatureSize = JwsUtils.GetMaxByteCount(header.Algorithm);

        int encodedBufferSize = encodedHeader.Length + encodedPayload.Length + 1 + signatureSize;

        using var encodedChars = new PooledArray<char>(encodedBufferSize);

        encodedHeader.CopyTo(encodedChars);
        encodedChars[encodedHeader.Length] = '.';
        encodedPayload.CopyTo(encodedChars.AsSpan(encodedHeader.Length + 1));

        using var asciiBytes = new PooledArray<byte>(Encoding.ASCII.GetMaxByteCount(encodedBufferSize));

        int asciiBytesCount = Encoding.ASCII.GetBytes(encodedChars.AsSpan(0, encodedHeader.Length + encodedPayload.Length + 1), asciiBytes);

        using var signatureBytes = new PooledArray<byte>(signatureSize);

        int encodedSignatureLength = 0;
        int signatureLength = 0;

        if (header.JsonWebKey is RsaJsonWebKey rsaJwk) {
            signatureLength = SignRSA(rsaJwk,
               header.Algorithm,
               asciiBytes.AsSpan(0, asciiBytesCount),
               signatureBytes);
        }
        else if (header.JsonWebKey is EcJsonWebKey ecJwk) {
            signatureLength = SignEC(ecJwk,
                header.Algorithm,
                asciiBytes.AsSpan(0, asciiBytesCount),
                signatureBytes);
        }

        encodedSignatureLength = Base64Url.Encode(signatureBytes.AsSpan(0, signatureLength), encodedChars.AsSpan(encodedHeader.Length + encodedPayload.Length + 1));

        return encodedChars.AsSpan(encodedHeader.Length + encodedPayload.Length + 1, encodedSignatureLength).ToString();
    }

    public static bool Verify<TJwk>(
        JsonWebHeader<TJwk> header,
        ReadOnlySpan<byte> signature,
        ReadOnlySpan<char> encodedHeader,
        ReadOnlySpan<char> encodedPayload) where TJwk : JsonWebKey {

        if (header.JsonWebKey is null) return true;

        int encodedBufferSize = encodedHeader.Length + encodedPayload.Length + 1;

        using var encodedChars = new PooledArray<char>(encodedBufferSize);

        encodedHeader.CopyTo(encodedChars);
        encodedChars[encodedHeader.Length] = '.';
        encodedPayload.CopyTo(encodedChars.AsSpan(encodedHeader.Length + 1));

        using var asciiBytes = new PooledArray<byte>(Encoding.ASCII.GetMaxByteCount(encodedBufferSize));

        int asciiBytesCount = Encoding.ASCII.GetBytes(encodedChars.AsSpan(0, encodedBufferSize), asciiBytes);

        if (header.JsonWebKey is RsaJsonWebKey rsaJwk) {

            return VerifyRSA(rsaJwk,
                header.Algorithm,
                signature,
                asciiBytes.AsSpan(0, asciiBytesCount));

        }
        else if (header.JsonWebKey is EcJsonWebKey ecJwk) {

            return VerifyEC(ecJwk,
                header.Algorithm,
                signature,
                asciiBytes.AsSpan(0, asciiBytesCount));

        }

        return false;
    }

    private static bool VerifyRSA(RsaJsonWebKey jwk,
        JsonSignAlgorithm algorithm,
        ReadOnlySpan<byte> signature,
        ReadOnlySpan<byte> data) {

        var rsaPadding = (algorithm is
               JsonSignAlgorithm.PS256 or
               JsonSignAlgorithm.PS384 or
               JsonSignAlgorithm.PS512) ?
            RSASignaturePadding.Pss :
            RSASignaturePadding.Pkcs1;

        var hashAlgorithm = JwsUtils.GetHashAlgorithmName(algorithm);

        if (jwk._rsa is null) {

            using var rsa = RSA.Create(new RSAParameters() {
                Modulus = jwk.Modulus,
                Exponent = jwk.Exponent,
            });

            return rsa.VerifyData(data,
                signature,
                hashAlgorithm,
                rsaPadding);

        }

        return jwk._rsa.VerifyData(data,
                signature,
                hashAlgorithm,
                rsaPadding);
    }

    private static bool VerifyEC(EcJsonWebKey jwk,
        JsonSignAlgorithm algorithm,
        ReadOnlySpan<byte> signature,
        ReadOnlySpan<byte> data) {

        var hashAlgorithm = JwsUtils.GetHashAlgorithmName(algorithm);

        if (jwk._ec is null) {

            var curve = JwsUtils.GetEllipticCurveName(jwk.Curve);

            using var ec = ECDsa.Create(new ECParameters() {
                Curve = curve,
                Q = {
                X = jwk.X,
                Y = jwk.Y,
            }
            });

            return ec.VerifyData(data,
                signature,
                hashAlgorithm);

        }

        return jwk._ec.VerifyData(data,
                signature,
                hashAlgorithm);


    }

    private static int SignRSA(
        RsaJsonWebKey jwk,
        JsonSignAlgorithm algorithm,
        ReadOnlySpan<byte> data,
        Span<byte> destination) {

        Debug.Assert(jwk._rsa is not null);

        var (rsaPadding, hashAlgorithm) = JwsUtils.GetGetRSASignaturePaddingAndHashAlgorithmName(algorithm);

        if (rsaPadding is null) return 0;

        jwk._rsa.TrySignData(data,
            destination,
            hashAlgorithm,
            rsaPadding,
            out var signatureLength);

        return signatureLength;

    }

    private static int SignEC(EcJsonWebKey jwk,
        JsonSignAlgorithm algorithm,
        ReadOnlySpan<byte> data,
        Span<byte> destination) {

        Debug.Assert(jwk._ec is not null);

        var hashAlgorithm = JwsUtils.GetHashAlgorithmName(algorithm);

        jwk._ec.TrySignData(data,
            destination,
            hashAlgorithm,
            out var signatureLength);

        return signatureLength;
    }
}
