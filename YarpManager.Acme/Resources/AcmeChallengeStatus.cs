using System.Runtime.Serialization;
using YarpManager.Acme.Attributes;

namespace YarpManager.Acme.Resources;

[StringEnum]
public enum AcmeChallengeStatus {

    [EnumMember(Value = "pending")]
    Pending,

    [EnumMember(Value = "processing")]
    Processing,

    [EnumMember(Value = "valid")]
    Valid,

    [EnumMember(Value = "invalid")]
    Invalid

}
