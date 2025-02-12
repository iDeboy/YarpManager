using System.Buffers;

namespace YarpManager.Common;
public readonly struct TempArray<T> : IDisposable {

    private readonly T[]? _array;

    public ref T this[int index] {
        get {

            if (_array is null)
                ArgumentNullException.ThrowIfNull(_array);

            return ref _array[index];
        }
    }

    public TempArray(int minimumLength) {
        _array = ArrayPool<T>.Shared.Rent(minimumLength);
    }

    public T[] AsArray() {

        if (_array is null)
            ArgumentNullException.ThrowIfNull(_array);

        return _array;
    }

    public T[] ToArray()
        => AsSpan().ToArray();

    public T[] ToArray(int start)
        => AsSpan(start).ToArray();

    public T[] ToArray(int start, int length)
        => AsSpan(start, length).ToArray();

    public Span<T> AsSpan()
        => _array.AsSpan();

    public Span<T> AsSpan(int start)
        => _array.AsSpan(start);

    public Span<T> AsSpan(int start, int length)
        => _array.AsSpan(start, length);

    public void Dispose() {

        if (_array is not null)
            ArrayPool<T>.Shared.Return(_array);

    }

    public static implicit operator Span<T>(TempArray<T> array)
        => array.AsSpan();
}
