using System.Text.Json.Serialization;

namespace YarpManager.Acme.Jws;

/// <summary>
/// JWS uses cryptographic algorithms to digitally hash 
/// the contents of the JWS Protected Header and the JWS Payload.
/// <br/>
/// See: <see cref="https://datatracker.ietf.org/doc/html/rfc7518#section-3.1"/>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<JsonHashAlgorithm>))]
public enum JsonHashAlgorithm {

    /// <summary>
    /// HMAC using SHA-256
    /// </summary>
    HS256,
    /// <summary>
    /// HMAC using SHA-384
    /// </summary>
    HS384,
    /// <summary>
    /// HMAC using SHA-512
    /// </summary>
    HS512,

}
