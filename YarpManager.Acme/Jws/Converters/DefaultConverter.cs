using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using YarpManager.Acme.Utils;

namespace YarpManager.Acme.Jws.Converters;
internal sealed class DefaultConverter<T> : JsonConverter<T> {
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return JsonSerializer.Deserialize<T>(ref reader, JsonUtils.SerializerOptionsWithModifiers);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) {
        JsonSerializer.Serialize(writer, value, JsonUtils.SerializerOptionsWithModifiers);
    }
}
