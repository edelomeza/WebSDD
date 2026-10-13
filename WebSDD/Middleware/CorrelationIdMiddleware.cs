namespace WebSDD.Middleware;

// T-003: CorrelationId por request para Serilog + respuesta.
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var id = context.Request.Headers[HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(id))
        {
            id = Guid.NewGuid().ToString("N");
        }

        context.Response.Headers[HeaderName] = id;

        using (Serilog.Context.LogContext.PushProperty("CorrelationId", id))
        {
            await next(context);
        }
    }
}
