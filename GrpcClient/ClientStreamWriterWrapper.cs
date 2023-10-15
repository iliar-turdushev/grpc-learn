using Grpc.Core;
using GrpcClient;
using Microsoft.Extensions.Logging;

internal class ClientStreamWriterWrapper<TRequest> : IClientStreamWriter<TRequest>
    where TRequest : class
{
    private readonly IClientStreamWriter<TRequest> _clientStreamWriter;
    private readonly ILogger<LoggingInterceptor> _logger;

    private int _messageCount;

    public ClientStreamWriterWrapper(
        IClientStreamWriter<TRequest> clientStreamWriter,
        ILogger<LoggingInterceptor> logger)
    {
        _clientStreamWriter = clientStreamWriter;
        _logger = logger;
    }

    public WriteOptions? WriteOptions
    {
        get => _clientStreamWriter.WriteOptions;
        set => _clientStreamWriter.WriteOptions = value;
    }

    public async Task CompleteAsync()
    {
        await _clientStreamWriter.CompleteAsync();

        _logger.LogInformation("{MessageCount} messages sent", _messageCount);
    }

    public async Task WriteAsync(TRequest message)
    {
        await _clientStreamWriter.WriteAsync(message);

        _messageCount++;

        if (message is HelloRequest helloRequest)
        {
            _logger.LogInformation("Message #{MessageNumber}: {Name}", _messageCount, helloRequest.Name);
        }
    }
}