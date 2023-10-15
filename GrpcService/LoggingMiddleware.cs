internal class LoggingMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public LoggingMiddleware(ILogger<LoggingMiddleware> logger, RequestDelegate next)
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        _logger.LogInformation("HTTP start: {Method} {Path}",
            httpContext.Request.Method, httpContext.Request.Path.Value);

        await _next(httpContext);

        _logger.LogInformation("HTTP end");
    }
}