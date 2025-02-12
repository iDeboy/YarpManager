using System.Text.Json.Serialization;

namespace YarpManager.Acme.Jws;

[JsonConverter(typeof(JsonStringEnumConverter<KeyType>))]
public enum KeyType {
    EC,
    RSA,
}
