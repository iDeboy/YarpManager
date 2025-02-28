using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using YarpManager.Acme.Jws;
using YarpManager.Acme.Jws.Converters;

namespace YarpManager.Acme.Utils;

internal static class JsonUtils {

    internal static readonly JsonSerializerOptions SerializerOptionsWithModifiers = new(JsonSerializerDefaults.Web) {

        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver() {
            Modifiers = {
                    Base64Url.Base64Modifier,
                    EnumConverterFactory.EnumModifier
            }
        }

    };


    public static readonly JsonSerializerOptions SerializerOptions = new(SerializerOptionsWithModifiers) {

        TypeInfoResolver = null,
        Converters = {
            new JwsConverterFactory(),
            new JwhConverterFactory(),
            new ClassBase64UrlConverterFactory(),
            new EnumConverterFactory(),
            new DefaultConverterFactory()
        }
    };

    public static readonly JsonSerializerOptions Base64SerializerOptions = new(SerializerOptionsWithModifiers) {

        TypeInfoResolver = null,
        Converters = {
            new Base64UrlConverterFactory()
        }

    };


}
