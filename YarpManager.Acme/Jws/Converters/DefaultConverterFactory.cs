using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YarpManager.Acme.Jws.Converters;
internal sealed class DefaultConverterFactory : JsonConverterFactory {

    public override bool CanConvert(Type typeToConvert) {
        return true;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) {

        return (JsonConverter?)Activator.CreateInstance(
                typeof(DefaultConverter<>).MakeGenericType(typeToConvert),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: null,
                culture: null);
    }
}
