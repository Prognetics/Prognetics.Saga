using System.Diagnostics.CodeAnalysis;

namespace Prognetics.Saga.Orchestrator.Contract.DTO;

public readonly struct EngineResult<TOutput>
{
    private readonly TOutput _output;
    private readonly bool _isSuccess;

    private EngineResult(
        bool isSuccess,
        TOutput output)
    {
        _isSuccess = isSuccess;
        _output = output;
    }

    public static EngineResult<TOutput> Fail() => new(false, default!);

    public static EngineResult<TOutput> Success(TOutput output) => new(true, output);

    public bool TryGetOutput([NotNullWhen(true)]out TOutput? output)
    {
        if (_isSuccess)
        {
            output = _output;
        }
        else
        {
            output = default;
        }

        return _isSuccess;
    }
}
