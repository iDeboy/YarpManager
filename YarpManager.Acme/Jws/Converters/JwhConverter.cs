using System.Text.Json;
using System.Text.Json.Serialization;
using YarpManager.Acme.Jws.Jwk;
using YarpManager.Acme.Utils;

namespace YarpManager.Acme.Jws.Converters;
internal sealed class JwhConverter<TJwk> : JsonConverter<JsonWebHeader<TJwk>> where TJwk : JsonWebKey {
    public override JsonWebHeader<TJwk>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return JsonSerializer.Deserialize<JsonWebHeader<TJwk>>(ref reader, JsonUtils.SerializerOptionsWithModifiers);
    }

    public override void Write(Utf8JsonWriter writer, JsonWebHeader<TJwk> value, JsonSerializerOptions options) {


        writer.WriteStartObject();
        {
            writer.WritePropertyName("alg");
            JsonSerializer.Serialize(writer, value.Algorithm, JsonUtils.SerializerOptions);

            if (value.Nonce is not null) {
                writer.WritePropertyName("nonce");
                JsonSerializer.Serialize(writer, value.Nonce, JsonUtils.SerializerOptions);
            }

            writer.WritePropertyName("url");
            JsonSerializer.Serialize(writer, value.Url, JsonUtils.SerializerOptions);

            if (value.KeyId is null) {
                writer.WritePropertyName("jwk");
                JsonSerializer.Serialize(writer, value.JsonWebKey, JsonUtils.SerializerOptions);
            }
            else {
                writer.WritePropertyName("kid");
                JsonSerializer.Serialize(writer, value.KeyId, JsonUtils.SerializerOptions);
            }

        }
        writer.WriteEndObject();

    }
}