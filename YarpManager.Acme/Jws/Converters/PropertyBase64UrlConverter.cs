using System.Text.Json;
using System.Text.Json.Serialization;

namespace YarpManager.Acme.Jws.Converters;
internal sealed class PropertyBase64UrlConverter<T> : JsonConverter<T> {

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return Base64Url.Read<T>(ref reader, typeToConvert, options);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) {
        Base64Url.Write(writer, value, options);
    }
}
