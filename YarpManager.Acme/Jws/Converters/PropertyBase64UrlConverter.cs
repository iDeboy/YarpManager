using System.Buffers;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using YarpManager.Acme.Utils;

namespace YarpManager.Acme.Jws.Converters;
internal sealed class PropertyBase64UrlConverter<T> : JsonConverter<T> {

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

        if (reader.TokenType is not JsonTokenType.String)
            throw new JsonException();

        var base64str = reader.GetString();

        if (typeToConvert == typeof(byte[])) {
            return (T)(object)Base64Url.DecodeBytes(base64str);
        }

        ReadOnlySpan<char> data = Base64Url.Decode(base64str);

        if (typeToConvert == typeof(string)) {

            var array = ArrayPool<char>.Shared.Rent(data.Length + 2);

            using var d0 = Deferer.Create(array =>
                ArrayPool<char>.Shared.Return(array), array);

            array[0] = '"';
            data.CopyTo(array.AsSpan(1));
            array[data.Length + 1] = '"';

            return JsonSerializer.Deserialize<T>(array.AsSpan(0, data.Length + 2), options);

        }

        return JsonSerializer.Deserialize<T>(data, options);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) {

        // Make this implementation a method on Base64Url static class
        Base64Url.Write(writer, value, options);

        return;
        if (value is null) return;

        ReadOnlySpan<char> data;
        if (value is byte[] bytes) {
            data = Base64Url.Encode(bytes);
        }
        else if (value is char[] chars) {
            data = Base64Url.Encode(chars);
        }
        else {

            ReadOnlySpan<char> json = value switch {
                string str => str,
                char ch => MemoryMarshal.CreateReadOnlySpan(ref ch, 1),
                _ => JsonSerializer.Serialize(value, options)
            };

            data = Base64Url.Encode(json);
        }

        writer.WriteStringValue(data);
    }
}
