using System.Runtime.Serialization;
using YarpManager.Acme.Attributes;

namespace YarpManager.Acme.Resources;

[StringEnum]
public enum AcmeAuthorizationStatus {

    [EnumMember(Value = "pending")]
    Pending,

    [EnumMember(Value = "valid")]
    Valid,

    [EnumMember(Value = "invalid")]
    Invalid,

    [EnumMember(Value = "deactivated")]
    Deactivated,

    [EnumMember(Value = "expired")]
    Expired,

    [EnumMember(Value = "revoked")]
    Revoked,

}
