namespace YarpManager.Acme.Resources;
public sealed class AcmeError {

    public readonly struct Id {

        public Id() { }

        public required string Type { get; init; }
        public required string Value { get; init; }

    }

    public required string Type { get; init; }
    public required string Detail { get; init; }

    public Id? Identifier { get; init; } = null;

    public AcmeError[] Subproblems { get; init; } = [];

}
