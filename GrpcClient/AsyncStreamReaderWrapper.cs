using Grpc.Core;
using GrpcClient;
using Microsoft.Extensions.Logging;

internal class AsyncStreamReaderWrapper<TResponse> : IAsyncStreamReader<TResponse>
    where TResponse : class
{
    private readonly IAsyncStreamReader<TResponse> _asyncStreamReader;
    private readonly ILogger<LoggingInterceptor> _logger;

    private int _messageCount;

    public AsyncStreamReaderWrapper(
        IAsyncStreamReader<TResponse> asyncStreamReader,
        ILogger<LoggingInterceptor> logger)
    {
        _asyncStreamReader = asyncStreamReader;
        _logger = logger;
    }

    public TResponse Current => _asyncStreamReader.Current;

    public async Task<bool> MoveNext(CancellationToken cancellationToken)
    {
        bool result = await _asyncStreamReader.MoveNext(cancellationToken);

        if (result)
        {
            _messageCount++;

            if (_asyncStreamReader.Current is HelloReply helloReply)
            {
                _logger.LogInformation("Message #{MessageNumber}: {Message}", _messageCount, helloReply.Message);
            }
        }
        else
        {
            _logger.LogInformation("End: {MessageCount} messages received", _messageCount);
        }

        return result;
    }
}