using System.Runtime.Serialization;
using YarpManager.Acme.Attributes;

namespace YarpManager.Acme.Resources;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc8555 [Page 48]
/// </summary>
[StringEnum]
public enum AcmeOrderStatus {

    [EnumMember(Value = "pending")]
    Pending,

    [EnumMember(Value = "ready")]
    Ready,

    [EnumMember(Value = "processing")]
    Processing,

    [EnumMember(Value = "valid")]
    Valid,

    [EnumMember(Value = "invalid")]
    Invalid

}
