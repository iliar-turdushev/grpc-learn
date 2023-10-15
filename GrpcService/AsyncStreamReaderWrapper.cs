using Grpc.Core;
using GrpcService;

internal class AsyncStreamReaderWrapper<TRequest> : IAsyncStreamReader<TRequest>
{
    private readonly IAsyncStreamReader<TRequest> _asyncStreamReader;
    private readonly ILogger<LoggingInterceptor> _logger;
    private int _messageCount;

    public AsyncStreamReaderWrapper(
        IAsyncStreamReader<TRequest> asyncStreamReader,
        ILogger<LoggingInterceptor> logger)
    {
        _asyncStreamReader = asyncStreamReader;
        _logger = logger;
    }

    public TRequest Current => _asyncStreamReader.Current;

    public async Task<bool> MoveNext(CancellationToken cancellationToken)
    {
        bool result = await _asyncStreamReader.MoveNext(cancellationToken);

        if (result)
        {
            _messageCount++;

            if (_asyncStreamReader.Current is HelloRequest helloRequest)
            {
                _logger.LogInformation("Message #{MessageNumber}: {Name}", _messageCount, helloRequest.Name);
            }
        }
        else
        {
            _logger.LogInformation("End: {MessageCount} messages received", _messageCount);
        }

        return result;
    }
}