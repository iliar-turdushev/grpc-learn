using Grpc.Core;
using Grpc.Core.Interceptors;
using GrpcClient;
using Microsoft.Extensions.Logging;

internal class LoggingInterceptor : Interceptor
{
    private readonly ILogger<LoggingInterceptor> _logger;

    public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
    {
        _logger = logger;
    }

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        where TRequest : class
        where TResponse : class
    {
        LogRequest(request, context);
        TResponse response = continuation(request, context);
        LogResponse(response);
        return response;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        LogRequest(request, context);

        AsyncUnaryCall<TResponse> call = continuation(request, context);

        Task<TResponse> responseAsync = HandleResponse(
            call.ResponseAsync,
            response => LogResponse(response));

        return new AsyncUnaryCall<TResponse>(
            responseAsync,
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose);
    }

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        LogRequest(request, context);

        AsyncServerStreamingCall<TResponse> call = continuation(request, context);

        return new AsyncServerStreamingCall<TResponse>(
            new AsyncStreamReaderWrapper<TResponse>(call.ResponseStream, _logger),
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose);
    }

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        _logger.LogInformation("Start: {Method}", context.Method.FullName);

        AsyncClientStreamingCall<TRequest, TResponse> call = continuation(context);

        Task<TResponse> responseAsync = HandleResponse(
            call.ResponseAsync,
            response => LogResponse(response));

        return new AsyncClientStreamingCall<TRequest, TResponse>(
            new ClientStreamWriterWrapper<TRequest>(call.RequestStream, _logger),
            responseAsync,
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose);
    }

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        _logger.LogInformation("Start: {Method}", context.Method.FullName);
        
        AsyncDuplexStreamingCall<TRequest, TResponse> call = continuation(context);
        
        return new AsyncDuplexStreamingCall<TRequest, TResponse>(
            new ClientStreamWriterWrapper<TRequest>(call.RequestStream, _logger),
            new AsyncStreamReaderWrapper<TResponse>(call.ResponseStream, _logger),
            call.ResponseHeadersAsync,
            call.GetStatus,
            call.GetTrailers,
            call.Dispose);
    }

    private static async Task<TResponse> HandleResponse<TResponse>(
        Task<TResponse> task,
        Action<TResponse> onResponseReceived)
    {
        TResponse response = await task;
        onResponseReceived(response);
        return response;
    }

    private void LogRequest<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context)
        where TRequest : class
        where TResponse : class
    {
        if (request is HelloRequest helloRequest)
        {
            _logger.LogInformation("Start: {Name} --> {Method}", helloRequest.Name, context.Method.FullName);
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
