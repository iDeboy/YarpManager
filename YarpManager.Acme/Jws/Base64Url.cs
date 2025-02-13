using System.Buffers;
using System.Buffers.Text;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using YarpManager.Acme.Jws.Converters;
using YarpManager.Acme.Utils;
using YarpManager.Common;

namespace YarpManager.Acme.Jws;
public static class Base64Url {

    private const char base64PadCharacter = '=';
    private const char base64Character62 = '+';
    private const char base64Character63 = '/';
    private const char base64UrlCharacter62 = '-';
    private const char base64UrlCharacter63 = '_';

    internal static void Base64Modifier(JsonTypeInfo jsonType) {

        if (jsonType.Kind is not JsonTypeInfoKind.Object) return;

        foreach (var property in jsonType.Properties) {

            var classAttrib = property.PropertyType
                .GetCustomAttribute<Base64UrlAttribute>(true);

            var isClassBase64 = classAttrib is not null && classAttrib.Enabled;

            var propAttrib = (Base64UrlAttribute?)property.AttributeProvider?
                .GetCustomAttributes(typeof(Base64UrlAttribute), true)
                .FirstOrDefault();

            bool isPropertyBase64 = propAttrib is not null && propAttrib.Enabled;

            bool shouldUseBase64 = isPropertyBase64 || (isClassBase64 && (propAttrib is null || propAttrib.Enabled));

            if (!shouldUseBase64) continue;

            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(PropertyBase64UrlConverter<>).MakeGenericType(property.PropertyType),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: null,
                culture: null)!;

            property.CustomConverter = converter;
        }



    }

    internal static T? Read<T>(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

        if (reader.TokenType is not JsonTokenType.String)
            throw new JsonException();

        var base64str = reader.GetString();

        if (typeToConvert == typeof(byte[])) {
            return (T)(object)DecodeBytes(base64str);
        }

        // Plus 2 just in case T is string or char, for starting and ending '"'
        // time 2 in case of escaping
        using var temp = new PooledArray<char>(2 + 2 * GetDecodedBytesCount(base64str, out _));

        ReadOnlySpan<char> data;
        if (typeToConvert == typeof(string) || typeToConvert == typeof(char)) {

            temp[0] = '"';

            int j = Decode(base64str, temp.AsSpan(1));

            // Escape temp.AsSpan(1, j)
            var escaped = JsonEncodedText.Encode(temp.AsSpan(1, j), options.Encoder);
            j = escaped.Value.Length;
            escaped.Value.AsSpan().CopyTo(temp.AsSpan(1));

            temp[j + 1] = '"';

            data = temp.AsSpan(0, j + 2);

        }
        else {

            int j = Decode(base64str, temp);

            data = temp.AsSpan(0, j);

        }

        if (typeToConvert == typeof(char[])) {
            return (T)(object)data.ToArray();
        }

        return JsonSerializer.Deserialize<T>(data, options);
    }

    internal static void Write<T>(Utf8JsonWriter writer, T value, JsonSerializerOptions options) {

        if (value is null) return;

        PooledArray<char> temp;

        ReadOnlySpan<char> encodedValue;
        if (value is byte[] bytes) {

            temp = new PooledArray<char>(GetEncodedCharsCount(bytes));

            int j = Encode(bytes, temp);

            encodedValue = temp.AsSpan(0, j);
        }
        else if (value is char[] chars) {

            temp = new PooledArray<char>(GetEncodedCharsCount(chars));

            int j = Encode(chars, temp);

            encodedValue = temp.AsSpan(0, j);

        }
        else {

            ReadOnlySpan<char> json = value switch {
                string str => str,
                char ch => MemoryMarshal.CreateReadOnlySpan(ref ch, 1),
                _ => JsonSerializer.Serialize(value, options)
            };

            temp = new PooledArray<char>(GetEncodedCharsCount(json));

            int j = Encode(json, temp);

            encodedValue = temp.AsSpan(0, j);
        }

        writer.WriteStringValue(encodedValue);

        temp.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetEncodedCharsCount(ReadOnlySpan<byte> data)
        => (data.Length + 2) / 3 * 4;

    public static int GetDecodedBytesCount(ReadOnlySpan<char> data, out int mod) {
        mod = data.Length % 4;
        return data.Length + (4 - mod) % 4;
    }

    // y = (x + 2) / 3 * 4
    // y * 3 = (x + 2) * 4
    // (y * 3) / 4 = x + 2
    // (y * 3) / 4 + 2 = x

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetEncodedCharsCount(ReadOnlySpan<char> data)
        => (AcmeUtils.Encoding.GetByteCount(data) + 2 + 1) / 3 * 4;

    public static string Encode(ReadOnlySpan<char> data) {

        if (data.IsEmpty) return string.Empty;

        var byteCount = AcmeUtils.Encoding.GetByteCount(data);
        var arrayToReturn = ArrayPool<byte>.Shared.Rent(byteCount + 1);

        if (!AcmeUtils.Encoding.TryGetBytes(data, arrayToReturn, out var i))
            return string.Empty;

        var bytes = arrayToReturn.AsSpan(0, i);

        var charCount = (bytes.Length + 2) / 3 * 4;
        var arrayToReturn2 = ArrayPool<char>.Shared.Rent(charCount);

        int j = Encode(bytes, arrayToReturn2);

        var chars = arrayToReturn2.AsSpan(0, j);
        var str = chars.ToString();

        bytes.Clear();
        ArrayPool<byte>.Shared.Return(arrayToReturn);

        chars.Clear();
        ArrayPool<char>.Shared.Return(arrayToReturn2);

        return str;
    }

    public static int Encode(ReadOnlySpan<char> data, Span<char> output) {

        if (data.IsEmpty) return 0;

        var byteCount = AcmeUtils.Encoding.GetByteCount(data);
        var arrayToReturn = ArrayPool<byte>.Shared.Rent(byteCount + 1);

        if (!AcmeUtils.Encoding.TryGetBytes(data, arrayToReturn, out var i))
            return 0;

        var bytes = arrayToReturn.AsSpan(0, i);

        int j = Encode(bytes, output);

        bytes.Clear();
        ArrayPool<byte>.Shared.Return(arrayToReturn);

        return j;
    }

    public static int Encode(ReadOnlySpan<char> data, Span<byte> output) {

        if (data.IsEmpty) return 0;

        var byteCount = AcmeUtils.Encoding.GetByteCount(data);
        var arrayToReturn = ArrayPool<byte>.Shared.Rent(byteCount + 1);

        if (!AcmeUtils.Encoding.TryGetBytes(data, arrayToReturn, out var i))
            return 0;

        var bytes = arrayToReturn.AsSpan(0, i);

        int j = Encode(bytes, output);

        bytes.Clear();
        ArrayPool<byte>.Shared.Return(arrayToReturn);

        return j;
    }

    public static byte[] EncodeBytes(ReadOnlySpan<char> data) {

        if (data.IsEmpty) return [];

        var byteCount = AcmeUtils.Encoding.GetByteCount(data);
        var arrayToReturn = ArrayPool<byte>.Shared.Rent(byteCount);

        if (!AcmeUtils.Encoding.TryGetBytes(data, arrayToReturn, out var i))
            return [];

        var bytes = arrayToReturn.AsSpan(0, i);
        var charCount = (bytes.Length + 2) / 3 * 4;
        var byteArr = new byte[charCount];

        int j = Encode(bytes, byteArr);

        bytes.Clear();
        ArrayPool<byte>.Shared.Return(arrayToReturn);

        return byteArr;
    }

    public static string Encode(ReadOnlySpan<byte> data) {

        var arrayToReturn = ArrayPool<char>.Shared.Rent(GetEncodedCharsCount(data));

        int j = Encode(data, arrayToReturn);

        var chars = arrayToReturn.AsSpan(0, j);
        var str = chars.ToString();

        chars.Clear();
        ArrayPool<char>.Shared.Return(arrayToReturn);

        return str;
    }

    public static byte[] EncodeBytes(ReadOnlySpan<byte> data) {

        var charCount = (data.Length + 2) / 3 * 4;
        var arrayToReturn = ArrayPool<byte>.Shared.Rent(charCount);

        int j = Encode(data, arrayToReturn);

        var bytes = arrayToReturn.AsSpan(0, j);
        var arr = bytes.ToArray();

        bytes.Clear();
        ArrayPool<byte>.Shared.Return(arrayToReturn);

        return arr;

    }

    public static int Encode(ReadOnlySpan<byte> data, Span<char> output) {

        int lengthmod3 = data.Length % 3;
        int limit = (data.Length - lengthmod3);
        ReadOnlySpan<byte> table = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_"u8;

        int i, j = 0;

        for (i = 0; i < limit; i += 3) {
            byte d0 = data[i];
            byte d1 = data[i + 1];
            byte d2 = data[i + 2];

            output[j + 0] = (char)table[d0 >> 2];
            output[j + 1] = (char)table[((d0 & 0x03) << 4) | (d1 >> 4)];
            output[j + 2] = (char)table[((d1 & 0x0f) << 2) | (d2 >> 6)];
            output[j + 3] = (char)table[d2 & 0x3f];
            j += 4;
        }

        i = limit;

        switch (lengthmod3) {
            case 2: {
                byte d0 = data[i];
                byte d1 = data[i + 1];

                output[j + 0] = (char)table[d0 >> 2];
                output[j + 1] = (char)table[((d0 & 0x03) << 4) | (d1 >> 4)];
                output[j + 2] = (char)table[(d1 & 0x0f) << 2];
                j += 3;
            }
            break;

            case 1: {
                byte d0 = data[i];

                output[j + 0] = (char)table[d0 >> 2];
                output[j + 1] = (char)table[(d0 & 0x03) << 4];
                j += 2;
            }
            break;

            //default or case 0: no further operations are needed.
        }

        return j;
    }

    public static int Encode(ReadOnlySpan<byte> data, Span<byte> output) {

        int lengthmod3 = data.Length % 3;
        int limit = (data.Length - lengthmod3);
        ReadOnlySpan<byte> table = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_"u8;

        int i, j = 0;

        for (i = 0; i < limit; i += 3) {
            byte d0 = data[i];
            byte d1 = data[i + 1];
            byte d2 = data[i + 2];

            output[j + 0] = table[d0 >> 2];
            output[j + 1] = table[((d0 & 0x03) << 4) | (d1 >> 4)];
            output[j + 2] = table[((d1 & 0x0f) << 2) | (d2 >> 6)];
            output[j + 3] = table[d2 & 0x3f];
            j += 4;
        }

        i = limit;

        switch (lengthmod3) {
            case 2: {
                byte d0 = data[i];
                byte d1 = data[i + 1];

                output[j + 0] = table[d0 >> 2];
                output[j + 1] = table[((d0 & 0x03) << 4) | (d1 >> 4)];
                output[j + 2] = table[(d1 & 0x0f) << 2];
                j += 3;
            }
            break;

            case 1: {
                byte d0 = data[i];

                output[j + 0] = table[d0 >> 2];
                output[j + 1] = table[(d0 & 0x03) << 4];
                j += 2;
            }
            break;

            //default or case 0: no further operations are needed.
        }

        return j;

    }

    public static string Decode(ReadOnlySpan<char> data) {

        if (data.IsEmpty) return string.Empty;

        return AcmeUtils.Encoding.GetString(DecodeBytes(data).AsSpan());
    }

    public static byte[] DecodeBytes(ReadOnlySpan<char> data) {

        if (data.IsEmpty) return [];

        int decodedLength = GetDecodedBytesCount(data, out int mod);

        if (mod == 1)
            throw new FormatException(data.ToString());

        bool needReplace = data.IndexOfAny(base64UrlCharacter62, base64UrlCharacter63) >= 0;

        var arrayToReturn = ArrayPool<byte>.Shared.Rent(decodedLength);
        Span<byte> output = arrayToReturn.AsSpan(0, decodedLength);

        int length = Decode(data, output, needReplace, decodedLength);

        var signature = output[..length].ToArray();

        ArrayPool<byte>.Shared.Return(arrayToReturn);

        return signature;

    }

    public static int Decode(ReadOnlySpan<char> data, Span<byte> output) {

        int decodedLength = GetDecodedBytesCount(data, out int mod);
        if (mod == 1)
            throw new FormatException(data.ToString());

        bool needReplace = data.IndexOfAny(base64UrlCharacter62, base64UrlCharacter63) >= 0;

        return Decode(data, output, needReplace, decodedLength);
    }

    public static int Decode(ReadOnlySpan<char> data, Span<char> output) {

        int decodedLength = GetDecodedBytesCount(data, out _);

        using var temp = new PooledArray<byte>(decodedLength);

        int j = Decode(data, temp);

        return AcmeUtils.Encoding.GetChars(temp.AsSpan(0, j), output);
    }

    private static ReadOnlySpan<char> HandlePaddingAndReplace(ReadOnlySpan<char> source, Span<char> charsSpan, bool needReplace) {
        source.CopyTo(charsSpan);
        if (source.Length < charsSpan.Length) {
            charsSpan[source.Length] = base64PadCharacter;
            if (source.Length + 1 < charsSpan.Length) {
                charsSpan[source.Length + 1] = base64PadCharacter;
            }
        }

        if (needReplace) {
            Span<char> remaining = charsSpan;
            int pos;
            while ((pos = remaining.IndexOfAny(base64UrlCharacter62, base64UrlCharacter63)) >= 0) {
                remaining[pos] = (remaining[pos] == base64UrlCharacter62) ? base64Character62 : base64Character63;
                remaining = remaining[(pos + 1)..];
            }
        }

        return charsSpan;
    }

    private static int Decode(ReadOnlySpan<char> data, Span<byte> output, bool needReplace, int decodedLength) {

        const int StackAllocThreshold = 512;
        char[]? arrayPoolChars = null;
        scoped Span<char> charsSpan = default;
        scoped ReadOnlySpan<char> source = data;

        if (needReplace || decodedLength != source.Length) {
            charsSpan = decodedLength <= StackAllocThreshold ?
                stackalloc char[StackAllocThreshold] :
                arrayPoolChars = ArrayPool<char>.Shared.Rent(decodedLength);
            charsSpan = charsSpan[..decodedLength];

            source = HandlePaddingAndReplace(source, charsSpan, needReplace);
        }

        byte[]? arrayPoolBytes = null;
        Span<byte> bytesSpan = decodedLength <= StackAllocThreshold ?
            stackalloc byte[StackAllocThreshold] :
            arrayPoolBytes = ArrayPool<byte>.Shared.Rent(decodedLength);

        int length = AcmeUtils.Encoding.GetBytes(source, bytesSpan);
        Span<byte> utf8Span = bytesSpan[..length];

        try {
            OperationStatus status = Base64.DecodeFromUtf8InPlace(utf8Span, out int bytesWritten);
            if (status != OperationStatus.Done)
                throw new FormatException(data.ToString());

            utf8Span[..bytesWritten].CopyTo(output);

            return bytesWritten;
        }
        finally {
            if (arrayPoolBytes is not null) {
                bytesSpan.Clear();
                ArrayPool<byte>.Shared.Return(arrayPoolBytes);
            }

            if (arrayPoolChars is not null) {
                charsSpan.Clear();
                ArrayPool<char>.Shared.Return(arrayPoolChars);
            }
        }
    }


}
