using System.Runtime.Serialization;
using YarpManager.Acme.Attributes;

namespace YarpManager.Acme.Resources;

/// <summary>
/// Represents the status of <see cref="AcmeAccount"/>.
/// </summary>
[StringEnum]
public enum AccountStatusType {
    /// <summary>
    /// The valid status.
    /// </summary>
    [EnumMember(Value = "valid")]
    Valid,

    /// <summary>
    /// The deactivated status, initiated by client.
    /// </summary>
    [EnumMember(Value = "deactivated")]
    Deactivated,

    /// <summary>
    /// The revoked status, initiated by server.
    /// </summary>
    [EnumMember(Value = "revoked")]
    Revoked,
}
