using System.Buffers;
using System.Runtime.CompilerServices;

namespace YarpManager.Common;
public readonly struct PooledArray<T> : IDisposable {

    private readonly T[]? _array;

    public ref T this[int index] {
        get {

            if (_array is null)
                ArgumentNullException.ThrowIfNull(_array);

            return ref _array[index];
        }
    }

    public PooledArray(int minimumLength) {
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

        Unsafe.AsRef(in _array) = null;
    }

    public static implicit operator Span<T>(PooledArray<T> array)
        => array.AsSpan();

    public static implicit operator ReadOnlySpan<T>(PooledArray<T> array)
        => array.AsSpan();
}
