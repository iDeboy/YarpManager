using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using YarpManager.Acme.Jws.Jwk;
using YarpManager.Acme.Utils;

namespace YarpManager.Acme.Jws.Converters;
internal sealed class JwsConverter<TJwk, TPayload> : JsonConverter<JsonWebSignature<TJwk, TPayload>> where TJwk : JsonWebKey {
    public override JsonWebSignature<TJwk, TPayload>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

        JsonWebHeader<TJwk>? header = default;
        string? encodedHeader = null;

        TPayload? payload = default;
        string? encodedPayload = null;

        string? signature = default;
        while (reader.Read()) {

            if (reader.TokenType is JsonTokenType.EndObject) break;

            if (reader.TokenType is JsonTokenType.StartObject) continue;

            if (reader.TokenType is not JsonTokenType.PropertyName)
                throw new JsonException();

            string propertyName = reader.GetString()!;
            if (!reader.Read()) break;

            if (reader.TokenType is not JsonTokenType.String)
                throw new JsonException();

            string json;

            switch (propertyName) {
                case "protected":
                    encodedHeader = reader.GetString()!;
                    json = Base64Url.Decode(encodedHeader);

                    header = JsonSerializer.Deserialize<JsonWebHeader<TJwk>>(json, JsonUtils.SerializerOptionsWithModifiers);
                    break;
                case "payload":

                    encodedPayload = reader.GetString()!;
                    json = Base64Url.Decode(encodedPayload);

                    if (typeof(TPayload) == typeof(EmptyPayload)) {
                        payload = (TPayload)(object)EmptyPayload.Instance;
                    }
                    else if (typeof(TPayload) == typeof(EmptyObjectPayload)) {
                        payload = (TPayload)(object)EmptyObjectPayload.Instance;
                    }
                    else {
                        payload = JsonSerializer.Deserialize<TPayload>(json, JsonUtils.SerializerOptionsWithModifiers);
                    }

                    break;
                case "signature":
                    signature = reader.GetString();

                    break;
            }
        }

        Debug.Assert(header is not null);
        Debug.Assert(payload is not null);
        Debug.Assert(signature is not null);

        if (!JwsSigner.Verify(header, Base64Url.DecodeBytes(signature), encodedHeader, encodedPayload))
            throw new JsonException("Invalid signature");

        return new JsonWebSignature<TJwk, TPayload>() {
            Protected = header,
            Payload = payload,
            Signature = signature
        };
    }

    public override void Write(Utf8JsonWriter writer, JsonWebSignature<TJwk, TPayload> value, JsonSerializerOptions options) {

        var header = value.Protected;

        var encodedHeader = JsonSerializer.Serialize(header, JsonUtils.Base64SerializerOptions);
        
        var encodedPayload = value.Payload switch {
            EmptyPayload or null => string.Empty, //Base64Url.Encode(string.Empty),
            EmptyObjectPayload => "e30", // Base64Url.Encode("{}")
            _ => JsonSerializer.Serialize(value.Payload, JsonUtils.Base64SerializerOptions).AsSpan().Trim('"'),
        };
        
        var encodedSignature = JwsSigner.Sign(header, encodedHeader, encodedPayload);

        writer.WriteStartObject();
        {
            writer.WriteString("protected", encodedHeader);
            writer.WriteString("payload", encodedPayload);
            writer.WriteString("signature", encodedSignature);
        }
        writer.WriteEndObject();

    }
}
