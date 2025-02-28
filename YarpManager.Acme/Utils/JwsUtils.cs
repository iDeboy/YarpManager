using System.Security.Cryptography;
using System.Text.Json;
using YarpManager.Acme.Jws;
using YarpManager.Acme.Jws.Jwk;
using YarpManager.Common;

namespace YarpManager.Acme.Utils;
internal static class JwsUtils {

    public static ECCurve GetEllipticCurveName(EllipticCurve curve) {

        return curve switch {
            EllipticCurve.P256 => ECCurve.NamedCurves.nistP256,
            EllipticCurve.P384 => ECCurve.NamedCurves.nistP384,
            EllipticCurve.P512 or
            EllipticCurve.P521 => ECCurve.NamedCurves.nistP521,
            _ => throw new NotSupportedException(),
        };

    }

    public static ECCurve GetEllipticCurveName(JsonSignAlgorithm algorithm) {

        return algorithm switch {
            JsonSignAlgorithm.ES256 => ECCurve.NamedCurves.nistP256,
            JsonSignAlgorithm.ES384 => ECCurve.NamedCurves.nistP384,
            JsonSignAlgorithm.ES512 => ECCurve.NamedCurves.nistP521,
            _ => throw new NotImplementedException(),
        };

    }

    public static EllipticCurve GetEllipticCurve(ECCurve curve) {

        var oidValue = curve.Oid.Value.AsSpan();

        if (oidValue.SequenceEqual(ECCurve.NamedCurves.nistP256.Oid.Value)) {
            return EllipticCurve.P256;
        }

        if (oidValue.SequenceEqual(ECCurve.NamedCurves.nistP384.Oid.Value)) {
            return EllipticCurve.P384;
        }

        if (oidValue.SequenceEqual(ECCurve.NamedCurves.nistP521.Oid.Value)) {
            return EllipticCurve.P521;
        }

        throw new NotSupportedException();
    }

    public static byte[] GetThumbprint<TJwk>(TJwk jwk) where TJwk : JsonWebKey {

        var json = JsonSerializer.Serialize(jwk, JsonUtils.SerializerOptions);

        var dataLength = AcmeUtils.Encoding.GetByteCount(json);

        using var tempBuffer = new PooledArray<byte>(dataLength);

        int i = AcmeUtils.Encoding.GetBytes(json, tempBuffer);

        return SHA256.HashData(tempBuffer.AsSpan(0, i));
    }

    public static HashAlgorithmName GetHashAlgorithmName(JsonSignAlgorithm algorithm) {

        return algorithm switch {
            //JsonSignAlgorithm.HS256 or
            JsonSignAlgorithm.RS256 or
            JsonSignAlgorithm.ES256 or
            JsonSignAlgorithm.PS256 => HashAlgorithmName.SHA256,
            //JsonSignAlgorithm.HS384 or
            JsonSignAlgorithm.RS384 or
            JsonSignAlgorithm.ES384 or
            JsonSignAlgorithm.PS384 => HashAlgorithmName.SHA384,
            //JsonSignAlgorithm.HS512 or
            JsonSignAlgorithm.RS512 or
            JsonSignAlgorithm.ES512 or
            JsonSignAlgorithm.PS512 => HashAlgorithmName.SHA512,
            _ => throw new NotImplementedException(),
        };

    }

    public static int GetMaxByteCount(JsonSignAlgorithm algorithm) {

        return algorithm switch {

            //JsonSignAlgorithm.HS256 => 32,
            //JsonSignAlgorithm.HS384 => 48,
            //JsonSignAlgorithm.HS512 => 64,

            JsonSignAlgorithm.ES256 or
            JsonSignAlgorithm.ES384 or
            JsonSignAlgorithm.RS256 or
            JsonSignAlgorithm.RS384 or
            JsonSignAlgorithm.PS256 or
            JsonSignAlgorithm.PS384 => 512,

            JsonSignAlgorithm.ES512 or
            JsonSignAlgorithm.RS512 or
            JsonSignAlgorithm.PS512 => 1024,

            _ => 2048,
        };
    }

    public static RSASignaturePadding? GetRSASignaturePadding(JsonSignAlgorithm algorithm) {
        return algorithm switch {
            JsonSignAlgorithm.PS256 or
            JsonSignAlgorithm.PS384 or
            JsonSignAlgorithm.PS512 => RSASignaturePadding.Pss,
            JsonSignAlgorithm.RS256 or
            JsonSignAlgorithm.RS384 or
            JsonSignAlgorithm.RS512 => RSASignaturePadding.Pkcs1,
            _ => null,
        };
    }

    public static (RSASignaturePadding?, HashAlgorithmName) GetGetRSASignaturePaddingAndHashAlgorithmName(JsonSignAlgorithm algorithm) {
        return algorithm switch {
            JsonSignAlgorithm.PS256 => (RSASignaturePadding.Pss, HashAlgorithmName.SHA256),
            JsonSignAlgorithm.PS384 => (RSASignaturePadding.Pss, HashAlgorithmName.SHA384),
            JsonSignAlgorithm.PS512 => (RSASignaturePadding.Pss, HashAlgorithmName.SHA512),
            JsonSignAlgorithm.RS256 => (RSASignaturePadding.Pkcs1, HashAlgorithmName.SHA256),
            JsonSignAlgorithm.RS384 => (RSASignaturePadding.Pkcs1, HashAlgorithmName.SHA384),
            JsonSignAlgorithm.RS512 => (RSASignaturePadding.Pkcs1, HashAlgorithmName.SHA512),
            JsonSignAlgorithm.ES256 => (null, HashAlgorithmName.SHA256),
            JsonSignAlgorithm.ES384 => (null, HashAlgorithmName.SHA384),
            JsonSignAlgorithm.ES512 => (null, HashAlgorithmName.SHA512),
            _ => throw new NotImplementedException(),
        };
    }

}
