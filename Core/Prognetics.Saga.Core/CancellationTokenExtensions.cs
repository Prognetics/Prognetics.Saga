using System.Runtime.CompilerServices;

namespace Prognetics.Saga.Core;
public static class CancellationTokenExtensions
{
    public static async Task Wait(this CancellationToken cancellationToken)
        => await new AwaitableCancellationToken(cancellationToken);

    private readonly struct AwaitableCancellationToken
    {
        private readonly CancellationToken _cancellationToken;

        public AwaitableCancellationToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        public CancellationTokenAwaiter GetAwaiter()
            => new(_cancellationToken);
    }


    private readonly struct CancellationTokenAwaiter : INotifyCompletion, ICriticalNotifyCompletion
    {
        private readonly CancellationToken _cancellationToken;

        public CancellationTokenAwaiter(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        public readonly object GetResult()
        {
            if (!IsCompleted)
            {
                throw new InvalidOperationException("The cancellation token has not yet been cancelled.");
            }
            return _cancellationToken;
        }

        public readonly bool IsCompleted => _cancellationToken.IsCancellationRequested;

        public void OnCompleted(Action continuation)
            => _cancellationToken.Register(continuation);

        public void UnsafeOnCompleted(Action continuation)
            => _cancellationToken.Register(continuation);
    }
}
