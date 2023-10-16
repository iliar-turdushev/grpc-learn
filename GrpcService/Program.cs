using GrpcService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureLogging(logging =>
{
    logging
        .ClearProviders()
        .AddSimpleConsole(options =>
        {
            options.TimestampFormat = "HH:mm:ss ";
        });
});

// Add services to the container
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<LoggingInterceptor>();
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseMiddleware<LoggingMiddleware>();

app.MapGrpcService<GreeterService>();

app.MapGet(
    "/",
    () => "Communication with gRPC endpoints must be made through a gRPC client. " +
          "To learn how to create a client, visit: " +
          "https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
