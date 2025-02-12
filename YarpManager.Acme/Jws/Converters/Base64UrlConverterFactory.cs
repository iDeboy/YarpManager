using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using YarpManager.Acme.Utils;

namespace YarpManager.Acme.Jws.Converters;
internal class Base64UrlConverterFactory : JsonConverterFactory {
    public override bool CanConvert(Type typeToConvert) {
        return true;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) {

        return (JsonConverter?)Activator.CreateInstance(
                typeof(ClassBase64UrlConverter<>).MakeGenericType(typeToConvert),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: [JsonUtils.SerializerOptions],
                culture: null);

    }
}
