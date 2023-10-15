using Grpc.Core;
using Grpc.Core.Interceptors;
using GrpcService;

internal class LoggingInterceptor : Interceptor
{
    private readonly ILogger<LoggingInterceptor> _logger;

    public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
        where TRequest : class
        where TResponse : class
    {
        LogRequest(request, context);
        TResponse response = await continuation(request, context);
        LogResponse(response);
        return response;
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
        where TRequest : class
        where TResponse : class
    {
        LogRequest(request, context);
        await continuation(request, new ServerStreamWriterWrapper<TResponse>(responseStream, _logger), context);
    }

    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
        where TRequest : class
        where TResponse : class
    {
        var response = await continuation(new AsyncStreamReaderWrapper<TRequest>(requestStream, _logger), context);
        LogResponse(response);
        return response;
    }

    public override Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        return continuation(
            new AsyncStreamReaderWrapper<TRequest>(requestStream, _logger),
            new ServerStreamWriterWrapper<TResponse>(responseStream, _logger),
            context);
    }

    private void LogRequest<TRequest>(TRequest request, ServerCallContext context)
        where TRequest : class
    {
        if (request is HelloRequest helloRequest)
        {
            _logger.LogInformation("Start: {Name} --> {Method}", helloRequest.Name, context.Method);
        }
    }

    private void LogResponse<TResponse>(TResponse response)
        where TResponse : class
    {
        if (response is HelloReply helloReply)
        {
            _logger.LogInformation("End: {Message}", helloReply.Message);
        }
    }
}
