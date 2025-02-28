using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace YarpManager.Common.Result;

/// <summary>
/// Represents extension methods for type <see cref="Result{T}"/>.
/// </summary>
public static class Result {

    /// <summary>
    /// If a result is successful, returns it, otherwise <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="result">The result.</param>
    /// <returns>Nullable value.</returns>
    public static T? OrNull<T, TError>(this in Result<T, TError> result)
        where T : struct
        where TError : struct
        => result.TryGet(out var value) ? value : null;

    /// <summary>
    /// Returns the second result if the first is unsuccessful.
    /// </summary>
    /// <param name="first">The first result.</param>
    /// <param name="second">The second result.</param>
    /// <typeparam name="T">The type of value.</typeparam>
    /// <returns>The second result if the first is unsuccessful; otherwise, the first result.</returns>
    public static ref readonly Result<T, TError> Coalesce<T, TError>(this in Result<T, TError> first, in Result<T, TError> second) => ref first.IsSuccessful ? ref first : ref second;

    /// <summary>
    /// Creates a new instance of <see cref="Result{T, TError}"/> from the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to be placed to the container.</param>
    /// <returns>The value encapsulated by <see cref="Result{T, TError}"/>.</returns>
    public static Result<T, TError> FromValue<T, TError>(T value) => new(value);

    /// <summary>
    /// Creates a new instance of <see cref="Result{T, TError}"/> from the specified error.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="error">The error to be placed to the container.</param>
    /// <returns>The error encapsulated by <see cref="Result{T, TError}"/>.</returns>
    public static Result<T, TError> FromError<T, TError>(TError error) => new(error);

    /// <summary>
    /// Creates a new instance of <see cref="Result{T, TError}"/> from the specified exception.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="e">The exception to be placed to the container.</param>
    /// <returns>The exception encapsulated by <see cref="Result{T, TError}"/>.</returns>
    public static Result<T, TError> FromException<T, TError>(Exception e) => new(e);
}

/// <summary>
/// <b>Based on: https://github.com/dotnet/dotNext/blob/master/src/DotNext/Result.cs</b>
/// <br/>
/// Represents a result of operation which can be actual result or error.
/// </summary>
/// <typeparam name="T">The type of the result.</typeparam>
/// <typeparam name="TError">The type of the error.</typeparam>
[StructLayout(LayoutKind.Auto)]
public readonly struct Result<T, TError> {

    private readonly T _value;
    private readonly TError _error;
    private readonly ExceptionDispatchInfo? _exception;

    /// <summary>
    /// Initializes a new successful result.
    /// </summary>
    /// <param name="value">The result value.</param>
    public Result(T value) {
        _value = value;
        Unsafe.SkipInit(out _error);
    }

    /// <summary>
    /// Initializes a new unsuccessful result.
    /// </summary>
    /// <param name="error">The error.</param>
    public Result(TError error) {
        _error = error;
        Unsafe.SkipInit(out _value);
    }

    /// <summary>
    /// Initializes a new unsuccessful result.
    /// </summary>
    /// <param name="error">The exception representing error. Cannot be <see langword="null"/>.</param>
    public Result(Exception error) {
        _exception = ExceptionDispatchInfo.Capture(error);
        Unsafe.SkipInit(out _value);
        Unsafe.SkipInit(out _error);
    }

    /// <summary>
    /// Extracts the actual result.
    /// </summary>
    public T Value {
        get {
            ValidateSuccess();
            return _value;
        }
    }

    /// <summary>
    /// Gets the error.
    /// </summary>
    public TError Error {
        get {
            ValidateError();
            return _error;
        }
    }

    public Exception? Exception => _exception?.SourceException;

    /// <summary>
    /// Gets a reference to the underlying value.
    /// </summary>
    /// <value>The reference to the result.</value>
    [UnscopedRef]
    [JsonIgnore]
    public ref readonly T ValueRef {
        get {
            ValidateSuccess();
            return ref _value;
        }
    }

    [UnscopedRef]
    [JsonIgnore]
    public ref readonly TError ErrorRef {
        get {
            ValidateError();
            return ref _error;
        }
    }

    /// <summary>
    /// Returns the value if present; otherwise return default value.
    /// </summary>
    /// <returns>The value, if present, otherwise <c>default</c>.</returns>
    public T? ValueOrDefault => _value;

    /// <summary>
    /// Indicates that the result is successful.
    /// </summary>
    /// <value><see langword="true"/> if this result is successful; <see langword="false"/> if this result represents an error or exception.</value>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccessful => _value is not null;

    /// <summary>
    /// Indicates that the result is unsuccessful.
    /// </summary>
    /// <value><see langword="true"/> if this result is unsuccessful; <see langword="false"/> if this result represents a successful result or exception.</value>
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsError => _error is not null;

    /// <summary>
    /// Indicates that the result is an exception.
    /// </summary>
    /// <value><see langword="true"/> if this result is an exception; <see langword="false"/> if this result represents a successful result or exception.</value>
    [MemberNotNullWhen(true, nameof(Exception))]
    public bool IsException => _exception is not null;

    /// <summary>
    /// Attempts to extract value from container if it is present.
    /// </summary>
    /// <param name="value">Extracted value.</param>
    /// <returns><see langword="true"/> if value is present; otherwise, <see langword="false"/>.</returns>
    public bool TryGet(out T? value) {
        value = _value;
        return IsSuccessful;
    }

    /// <summary>
    /// Returns the value if present; otherwise return default value.
    /// </summary>
    /// <param name="defaultValue">The value to be returned if this result is unsuccessful.</param>
    /// <returns>The value, if present, otherwise <paramref name="defaultValue"/>.</returns>
    public T? Or(T? defaultValue) => IsSuccessful ? _value : defaultValue;

    public Result<U, TError> To<U>() {

        if (IsError) return new Result<U, TError>(Error);

        if (IsException) return new Result<U, TError>(Exception);

        throw new InvalidCastException();

    }

    [DoesNotReturn]
    [StackTraceHidden]
    public Exception Throw() {

        if (IsException) _exception!.Throw();

        var exception = new UndefinedResultException<TError>(Error);
        throw exception;
        return exception;
    }

    /// <summary>
    /// Returns the value if present; otherwise return default value.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <param name="defaultValue">The value to be returned if this result is unsuccessful.</param>
    /// <returns>The value, if present, otherwise <paramref name="defaultValue"/>.</returns>
    public static T? operator |(in Result<T, TError> result, T? defaultValue)
        => result.Or(defaultValue);

    /// <summary>
    /// Tries to return successful result.
    /// </summary>
    /// <param name="x">The first container.</param>
    /// <param name="y">The second container.</param>
    /// <returns>The first successful result.</returns>
    public static Result<T, TError> operator |(in Result<T, TError> x, in Result<T, TError> y)
        => x.IsSuccessful ? x : y;

    /// <summary>
    /// Converts value into the result.
    /// </summary>
    /// <param name="result">The result to be converted.</param>
    /// <returns>The result representing <paramref name="result"/> value.</returns>
    public static implicit operator Result<T, TError>(T result) => new(result);

    /// <summary>
    /// Extracts actual result.
    /// </summary>
    /// <param name="result">The result object.</param>
    public static explicit operator T(in Result<T, TError> result) => result.Value;

    /// <summary>
    /// Converts error into the result.
    /// </summary>
    /// <param name="error">The error to be converted.</param>
    /// <returns>The result representing <paramref name="error"/> error.</returns>
    public static implicit operator Result<T, TError>(TError error) => new(error);

    /// <summary>
    /// Extracts actual error.
    /// </summary>
    /// <param name="result">The result object.</param>
    public static explicit operator TError(in Result<T, TError> result) => result.Error;

    /// <summary>
    /// Indicates that both results are successful.
    /// </summary>
    /// <param name="left">The first result to check.</param>
    /// <param name="right">The second result to check.</param>
    /// <returns><see langword="true"/> if both results are successful; otherwise, <see langword="false"/>.</returns>
    public static bool operator &(in Result<T, TError> left, in Result<T, TError> right) => left.IsSuccessful && right.IsSuccessful;

    /// <summary>
    /// Indicates that the result is successful.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <returns><see langword="true"/> if this result is successful; <see langword="false"/> if this result represents exception.</returns
    public static bool operator true(in Result<T, TError> result) => result.IsSuccessful;

    /// <summary>
    /// Indicates that the result represents error.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <returns><see langword="false"/> if this result is successful; <see langword="true"/> if this result represents exception.</returns>
    public static bool operator false(in Result<T, TError> result) => !result.IsSuccessful;

    private void ValidateSuccess() {

        _exception?.Throw();
        if (_error is not null) throw new InvalidOperationException("Cannot extract value from error.");

    }

    private void ValidateError() {

        _exception?.Throw();
        if (_value is not null) throw new InvalidOperationException("Result is not an error.");

    }

    /// <summary>
    /// Returns textual representation of this object.
    /// </summary>
    /// <returns>The textual representation of this object.</returns>
    public override string ToString() {

        if (IsSuccessful) return Value.ToString() ?? string.Empty;

        if (IsError) return Error.ToString() ?? string.Empty;

        if (IsException) return Exception.ToString();

        return base.ToString() ?? string.Empty;
    }
}
