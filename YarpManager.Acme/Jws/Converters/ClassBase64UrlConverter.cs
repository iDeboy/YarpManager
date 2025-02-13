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
        return Base64Url.Read<T>(ref reader, typeToConvert, _options);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) {
        Base64Url.Write(writer, value, _options);
    }
}
