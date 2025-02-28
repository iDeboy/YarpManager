using System.Text.Json.Serialization;

namespace YarpManager.Acme.Resources;
public sealed class AcmeError : ExtensionMembers {

    [JsonRequired]
    [JsonPropertyName("type")]
    public required AcmeErrorType Type { get; init; }

    [JsonRequired]
    [JsonPropertyName("detail")]
    public required string Detail { get; init; }

    [JsonPropertyName("subproblems")]
    public SubProblem[]? Subproblems { get; init; }

    public sealed class SubProblem : ExtensionMembers {

        [JsonRequired]
        [JsonPropertyName("type")]
        public required AcmeErrorType Type { get; init; }

        [JsonRequired]
        [JsonPropertyName("detail")]
        public required string Detail { get; init; }

        [JsonPropertyName("identifier")]
        public AcmeErrorIdentifier? Identifier { get; init; }

    }

}
