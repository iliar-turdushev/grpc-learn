using Grpc.Core;
using GrpcService;

internal class ServerStreamWriterWrapper<TResponse> : IServerStreamWriter<TResponse>
{
    private readonly IServerStreamWriter<TResponse> _serverStreamWriter;
    private readonly ILogger<LoggingInterceptor> _logger;
    private int _messageCount;

    public ServerStreamWriterWrapper(
        IServerStreamWriter<TResponse> serverStreamWriter,
        ILogger<LoggingInterceptor> logger)
    {
        _serverStreamWriter = serverStreamWriter;
        _logger = logger;
    }

    public WriteOptions WriteOptions
    {
        get => _serverStreamWriter.WriteOptions;
        set => _serverStreamWriter.WriteOptions = value;
    }

    public async Task WriteAsync(TResponse message)
    {
        await _serverStreamWriter.WriteAsync(message);

        if (message is HelloReply helloReply)
        {
            _messageCount++;
            _logger.LogInformation("Message #{MessageNumber}: {Message}", _messageCount, helloReply.Message);
        }
    }
}
