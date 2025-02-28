using System.Diagnostics.CodeAnalysis;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using YarpManager.Acme.Jws.Jwk;
using YarpManager.Acme.Utils;
using YarpManager.Common;

namespace YarpManager.Acme.Jws.Keys;
public abstract class AsymmetricKeyInfo : IDisposable {

    public static AsymmetricKeyInfo Create(JsonSignAlgorithm algorithm) {

        return algorithm switch {
            JsonSignAlgorithm.RS256 or
            JsonSignAlgorithm.RS384 or
            JsonSignAlgorithm.RS512 or
            JsonSignAlgorithm.PS256 or
            JsonSignAlgorithm.PS384 or
            JsonSignAlgorithm.PS512 => new RsaKeyInfo(RSA.Create(), algorithm),
            JsonSignAlgorithm.ES256 or
            JsonSignAlgorithm.ES384 or
            JsonSignAlgorithm.ES512 => new EcKeyInfo(ECDsa.Create(JwsUtils.GetEllipticCurveName(algorithm)), algorithm),
            _ => throw new NotSupportedException($"Unsuported algorithm {algorithm}."),
        };

    }

    public static bool TryLoadFromFile(string path, [NotNullWhen(true)] out AsymmetricKeyInfo? key) {

        key = null;
        PooledArray<byte> tempBuff = default;
        int j = 0;
        try {

            using var readStream = File.OpenRead(path);

            var algorithm = (JsonSignAlgorithm)readStream.ReadByte();

            if (algorithm < JsonSignAlgorithm.RS256
                && algorithm > JsonSignAlgorithm.PS512) return false;

            tempBuff = new PooledArray<byte>((int)readStream.Length - 1);

            int i = readStream.Read(tempBuff);
            if (i < 1) return false;

            key = Create(algorithm);
            key.Key.ImportPkcs8PrivateKey(tempBuff, out j);

            if (j != i) {
                key.Dispose();
                key = null;
                return false;
            }

        }
        catch (Exception) {
            key?.Dispose();
            key = null;
            return false;
        }
        finally {
            tempBuff.AsSpan(0, j).Clear();
            tempBuff.Dispose();
        }

        return true;
    }

    public JsonSignAlgorithm Algorithm { get; }
    public abstract AsymmetricAlgorithm Key { get; }
    public abstract JsonWebKey JsonWebKey { get; }
    public abstract byte[] Thumbprint { get; }

    protected AsymmetricKeyInfo(JsonSignAlgorithm algorithm) {
        Algorithm = algorithm;
    }

    
    public abstract CertificateRequest CreateEmptyCertificateRequest(X500DistinguishedName subjectName);   
    public bool SaveToFile(string path) {

        PooledArray<byte> tempBuff = default;
        int i = 0;
        try {

            using var writeStream = File.OpenWrite(path);
            writeStream.WriteByte((byte)Algorithm);

            tempBuff = new PooledArray<byte>(2700);

            if (!Key.TryExportPkcs8PrivateKey(tempBuff, out i)) {
                writeStream.Flush();
                return false;
            }
            writeStream.Write(tempBuff.AsSpan(0, i));
            return true;

        }
        catch (Exception) {
            return false;
        }
        finally {
            tempBuff.AsSpan(0, i).Clear();
            tempBuff.Dispose();
        }

    }
    public void Dispose() {

        Key.Dispose();

        GC.SuppressFinalize(this);
    }

}
