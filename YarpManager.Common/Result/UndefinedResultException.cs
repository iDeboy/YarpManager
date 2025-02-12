namespace YarpManager.Common.Result;

[Serializable]
public sealed class UndefinedResultException<TError> : Exception {
    internal UndefinedResultException(TError error)
        : base(error?.ToString())
        => Error = error;

    /// <summary>
    /// Gets the associated error with the exception.
    /// </summary>
    public TError Error { get; }
}
