using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using YarpManager.Acme.Attributes;
using YarpManager.Acme.Utils;

namespace YarpManager.Acme.Jws.Converters;
internal sealed class ClassBase64UrlConverterFactory : JsonConverterFactory {
    public override bool CanConvert(Type typeToConvert) {

        var attrib = typeToConvert.GetCustomAttribute<Base64UrlAttribute>(true);

        return attrib is not null && attrib.Enabled;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) {

        return (JsonConverter?)Activator.CreateInstance(
                typeof(ClassBase64UrlConverter<>).MakeGenericType(typeToConvert),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: [JsonUtils.SerializerOptionsWithModifiers],
                culture: null);

    }
}
