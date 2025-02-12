using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YarpManager.Acme.Jws.Converters;
internal sealed class JwhConverterFactory : JsonConverterFactory {
    public override bool CanConvert(Type typeToConvert) {

        if (!typeToConvert.IsGenericType) return false;

        if (typeToConvert.GetGenericTypeDefinition() == typeof(JsonWebHeader<>))
            return true;

        return false;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) {

        var jwkType = typeToConvert.GetGenericArguments()[0];

        return (JsonConverter)Activator.CreateInstance(
                    typeof(JwhConverter<>).MakeGenericType(jwkType),
                    BindingFlags.Instance | BindingFlags.Public,
                    binder: null,
                    args: null,
                    culture: null)!;

    }
}
