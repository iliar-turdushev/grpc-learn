using Grpc.Core;

namespace GrpcService.Services;

public class GreeterService : Greeter.GreeterBase
{
    public override Task<HelloReply> SayHello(
        HelloRequest request,
        ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }

    public override async Task SayHelloServerStream(
        HelloRequest request,
        IServerStreamWriter<HelloReply> responseStream,
        ServerCallContext context)
    {
        await responseStream.WriteAsync(new HelloReply
        {
            Message = "Hello " + request.Name + " #1"
        });

        await responseStream.WriteAsync(new HelloReply
        {
            Message = "Hello " + request.Name + " #2"
        });
    }

    public override async Task<HelloReply> SayHelloClientStream(
        IAsyncStreamReader<HelloRequest> requestStream,
        ServerCallContext context)
    {
        var names = new List<string>();

        await foreach (var request in requestStream.ReadAllAsync())
        {
            names.Add(request.Name);
        }

        return new HelloReply
        {
            Message = "Hello " + string.Join(" & ", names)
        };
    }

    public override async Task SayHelloDuplexStream(
        IAsyncStreamReader<HelloRequest> requestStream,
        IServerStreamWriter<HelloReply> responseStream,
        ServerCallContext context)
    {
        await foreach (var message in requestStream.ReadAllAsync())
        {
            await responseStream.WriteAsync(new HelloReply
            {
                Message = $"Hello {message.Name}"
            });
        }
    }
}
