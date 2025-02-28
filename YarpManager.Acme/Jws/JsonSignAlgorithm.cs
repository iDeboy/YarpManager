using YarpManager.Acme.Attributes;

namespace YarpManager.Acme.Jws;

/// <summary>
/// JWS uses cryptographic algorithms to digitally sign 
/// the contents of the JWS Protected Header and the JWS Payload.
/// <br/>
/// See: <see cref="https://datatracker.ietf.org/doc/html/rfc7518#section-3.1"/>
/// </summary>
[StringEnum]
public enum JsonSignAlgorithm : byte {

    /// <summary>
    /// RSASSA-PKCS1-v1_5 using SHA-256      
    /// </summary>
    RS256,
    /// <summary>
    /// RSASSA-PKCS1-v1_5 using SHA-384
    /// </summary>
    RS384,
    /// <summary>
    /// ECDSA using P-384 and SHA-384
    /// </summary>
    ES384,
    /// <summary>
    /// RSASSA-PKCS1-v1_5 using SHA-512
    /// </summary>
    RS512,
    /// <summary>
    /// RSASSA-PSS using SHA-256 and MGF1 with SHA-256
    /// </summary>
    PS256,
    /// <summary>
    /// RSASSA-PSS using SHA-384 and MGF1 with SHA-384
    /// </summary>
    PS384,
    /// <summary>
    /// ECDSA using P-256 and SHA-256
    /// </summary>
    ES256,
    /// <summary>
    /// ECDSA using P-521 and SHA-512
    /// </summary>
    ES512,
    /// <summary>
    /// RSASSA-PSS using SHA-512 and MGF1 with SHA-512
    /// </summary>
    PS512,

}
