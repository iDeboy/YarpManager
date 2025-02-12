namespace YarpManager.Acme.Jws;
internal readonly struct EmptyPayload {

    public static readonly EmptyPayload Instance = default;

    public override string ToString() {
        return string.Empty;
    }

}
