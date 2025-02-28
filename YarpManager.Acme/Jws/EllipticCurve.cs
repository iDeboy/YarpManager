using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using YarpManager.Acme.Attributes;

namespace YarpManager.Acme.Jws;

[StringEnum]
public enum EllipticCurve {

    [EnumMember(Value = "P-256")]
    P256,

    [EnumMember(Value = "P-384")]
    P384,

    [EnumMember(Value = "P-512")]
    P512,

    [EnumMember(Value = "P-521")]
    P521,

    //// oid = 1.3.101.112
    //[EnumMember(Value = "Ed25519")]
    //Ed25519,

    //// oid = 1.3.101.113
    //[EnumMember(Value = "Ed448")]
    //Ed448,

}
