using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace YarpManager.Common;
public static class Deferer {

    public static Deferer<TArg> Create<TArg>(Action<TArg> func, TArg arg)
        => new(func, arg);

    public static AsyncDeferer<TArg> CreateAsync<TArg>(Func<TArg, ValueTask> func, TArg arg)
        => new(func, arg);

}

public readonly struct Deferer<TArg> : IDisposable {

    private readonly TArg? _arg;
    private readonly Action<TArg>? _func;

    public Deferer(Action<TArg> func, TArg arg) {
        _func = func;
        _arg = arg;
    }

    public void Dispose() {

        _func?.Invoke(_arg!);

        Unsafe.AsRef(in _func) = null;
        Unsafe.AsRef(in _arg) = default;

    }
}

public readonly struct AsyncDeferer<TArg> : IAsyncDisposable {

    private readonly TArg? _arg;
    private readonly Func<TArg, ValueTask>? _func;

    public AsyncDeferer(Func<TArg, ValueTask> func, TArg arg) {
        _func = func;
        _arg = arg;
    }

    public async ValueTask DisposeAsync() {

        if (_func is null) return;

        Debug.Assert(_arg is not null);

        await _func.Invoke(_arg);

        Unsafe.AsRef<Func<TArg, ValueTask>?>(in _func) = null;
        Unsafe.AsRef(in _arg) = default;

    }
}