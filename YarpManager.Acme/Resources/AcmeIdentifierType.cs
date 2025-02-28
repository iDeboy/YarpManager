using System.Runtime.Serialization;
using YarpManager.Acme.Attributes;

namespace YarpManager.Acme.Resources;

[StringEnum]
public enum AcmeIdentifierType {

    [EnumMember(Value = "dns")]
    Dns

}
