using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using YarpManager.Acme.Utils;

namespace YarpManager.Acme.Jws.Converters;
internal sealed class ClassBase64UrlConverter<T> : JsonConverter<T> {

    //private readonly bool _enabled;

    //public ClassBase64UrlConverter(bool enabled) {
    //    _enabled = enabled;
    //}
    private readonly JsonSerializerOptions _options;

    public ClassBase64UrlConverter(JsonSerializerOptions options) {
        _options = options;
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

        //if (!_enabled)
        //    return JsonSerializer.Deserialize<T>(ref reader, JsonUtils.SerializerOptionsWithModifiers);

        if (reader.TokenType is not JsonTokenType.String)
            throw new JsonException();

        var base64str = reader.GetString();

        var data = Base64Url.Decode(base64str);

        return JsonSerializer.Deserialize<T>(data, _options);

        // return JsonSerializer.Deserialize<T>(data, JsonUtils.SerializerOptionsWithModifiers);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) {

        // var json = JsonSerializer.Serialize(value, JsonUtils.SerializerOptionsWithModifiers);

        if (value is null) return;

        ReadOnlySpan<char> json = value switch {
            string str => str,
            char ch => MemoryMarshal.CreateReadOnlySpan(ref ch, 1),
            _ => JsonSerializer.Serialize(value, _options)
        };

        json = Base64Url.Encode(json);
        //if (_enabled) {
        //    json = Base64Url.Encode(json);
        //}

        // writer.WriteRawValue(json, true);
        writer.WriteStringValue(json);
    }
}
