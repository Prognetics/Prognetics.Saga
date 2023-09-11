using System.Diagnostics.CodeAnalysis;

namespace Prognetics.Saga.Orchestrator.Contract.DTO;

public readonly struct EngineResult
{
    private readonly EngineOutput? _output;
    private readonly bool _isSuccess;

    private EngineResult(
        bool isSuccess,
        EngineOutput? output)
    {
        _isSuccess = isSuccess;
        _output = output;
    }

    public static EngineResult Fail() => new(false, null);

    public static EngineResult Success(EngineOutput output) => new(true, output);

    public bool TryGetOutput([NotNullWhen(true)]out EngineOutput? output)
    {
        if (_isSuccess)
        {
            output = _output;
        }
        else
        {
            output = null;
        }

        return _isSuccess;
    }
}
