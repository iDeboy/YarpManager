using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YarpManager.Acme.Jws.Converters;
internal sealed class JwsConverterFactory : JsonConverterFactory {
    public override bool CanConvert(Type typeToConvert) {

        if (!typeToConvert.IsGenericType) return false;

        if (typeToConvert.GetGenericTypeDefinition() == typeof(JsonWebSignature<,>))
            return true;

        return false;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) {

        var jwkType = typeToConvert.GetGenericArguments()[0];
        var payloadType = typeToConvert.GetGenericArguments()[1];

        return (JsonConverter)Activator.CreateInstance(
                    typeof(JwsConverter<,>).MakeGenericType(jwkType, payloadType),
                    BindingFlags.Instance | BindingFlags.Public,
                    binder: null,
                    args: null,
                    culture: null)!;

    }
}
