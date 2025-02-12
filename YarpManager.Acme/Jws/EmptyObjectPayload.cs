namespace YarpManager.Acme.Jws;

internal readonly struct EmptyObjectPayload {

    public static readonly EmptyObjectPayload Instance = default;

    public override string ToString() {
        return "{}";
    }

}
