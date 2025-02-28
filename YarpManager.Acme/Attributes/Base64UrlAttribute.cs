namespace YarpManager.Acme.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class Base64UrlAttribute : Attribute {

    /// <summary>
    /// Enables encode and decode the Property or Field on Base64Url during Serialization and Deserealization
    /// </summary>
    public bool Enabled { get; }


    public Base64UrlAttribute(bool enabled = true) {

        Enabled = enabled;

    }

}