using System.Text.Json;
using System.Text.Json.Serialization;

namespace YarpManager.Acme.Resources;
public abstract class ExtensionMembers {

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? ExtraData { get; set; }

}
