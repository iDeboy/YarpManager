using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using YarpManager.Acme.Attributes;

namespace YarpManager.Acme.Jws.Converters;
internal sealed class EnumConverterFactory : JsonConverterFactory {
    public override bool CanConvert(Type typeToConvert) {
        return typeToConvert.IsEnum;
    }

    public static void EnumModifier(JsonTypeInfo jsonType) {

        if (jsonType.Kind is not JsonTypeInfoKind.Object) return;

        foreach (var property in jsonType.Properties) {

            if (!property.PropertyType.IsEnum) continue;

            var enumAttrib = property.PropertyType
                .GetCustomAttribute<StringEnumAttribute>(false);

            var converter = (JsonConverter?)Activator.CreateInstance(
                    typeof(EnumConverter<>).MakeGenericType(property.PropertyType),
                    BindingFlags.Instance | BindingFlags.Public,
                    binder: null,
                    args: [enumAttrib is not null],
                    culture: null);

            property.CustomConverter = converter;


        }

    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) {

        var attrib = typeToConvert.GetCustomAttribute<StringEnumAttribute>();

        return (JsonConverter?)Activator.CreateInstance(
                typeof(EnumConverter<>).MakeGenericType(typeToConvert),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: [attrib is not null],
                culture: null);
    }
}
