using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using GrpcClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddSimpleConsole();
});
var logger = loggerFactory.CreateLogger<Program>();

// Create and configure a gRPC channel
#if true
using var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging
            .ClearProviders()
            .AddSimpleConsole()
            .AddFilter("System.Net.Http.HttpClient", LogLevel.None);
    })
    .ConfigureServices((context, services) =>
    {
        // TODO: Figure out why this line is needed. According to the docs, it shouldn't be.
        // https://learn.microsoft.com/en-us/aspnet/core/grpc/clientfactory?view=aspnetcore-7.0#configure-interceptors
        services.AddSingleton<LoggingInterceptor>();

        services
            .AddGrpcClient<Greeter.GreeterClient>(options =>
            {
                options.Address = new Uri("https://localhost:7254");
            })
            .AddInterceptor<LoggingInterceptor>();
    })
    .Build();

var client = host.Services.GetRequiredService<Greeter.GreeterClient>();
#else
var channel = GrpcChannel.ForAddress("https://localhost:7254");

var invoker = channel.Intercept(new LoggingInterceptor(
    loggerFactory.CreateLogger<LoggingInterceptor>()));

var client = new Greeter.GreeterClient(invoker);
#endif

// 1. BlockingUnaryCall
logger.LogInformation("========== BlockingUnaryCall ==========");
client.SayHello(new HelloRequest
{
    Name = "BlockingUnaryCall"
});

// 2. AsyncUnaryCall
logger.LogInformation("========== AsyncUnaryCall ==========");
await client.SayHelloAsync(new HelloRequest
{
    Name = "AsyncUnaryCall"
});

// 3. AsyncServerStreamingCall
logger.LogInformation("========== AsyncServerStreamingCall ==========");
var serverStreamCall = client.SayHelloServerStream(new HelloRequest
{
    Name = "AsyncServerStreamingCall"
});

await foreach (var streamCall in serverStreamCall.ResponseStream.ReadAllAsync())
{
}

// 4. AsyncClientStreamingCall
logger.LogInformation("========== AsyncClientStreamingCall ==========");
var clientStreamCall = client.SayHelloClientStream();

await clientStreamCall.RequestStream.WriteAsync(new HelloRequest
{
    Name = "Client #1"
});
await clientStreamCall.RequestStream.WriteAsync(new HelloRequest
{
    Name = "Client #2"
});

await clientStreamCall.RequestStream.CompleteAsync();

// Wait for the response
var x = await clientStreamCall;

// 5. AsyncDuplexStreamingCall
logger.LogInformation("========== AsyncDuplexStreamingCall ==========");
var duplexStreamCall = client.SayHelloDuplexStream();

await duplexStreamCall.RequestStream.WriteAsync(new HelloRequest
{
    Name = "Client Duplex #1"
});
await duplexStreamCall.ResponseStream.MoveNext();

await duplexStreamCall.RequestStream.WriteAsync(new HelloRequest
{
    Name = "Client Duplex #2"
});
await duplexStreamCall.ResponseStream.MoveNext();

await duplexStreamCall.RequestStream.CompleteAsync();
