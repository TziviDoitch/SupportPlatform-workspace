using Serilog.Context;

namespace SupportPlatform.Api.Middleware;

// Gives every request a correlation id (from X-Correlation-Id or generated): echoed on the
// response, pushed onto every Serilog line, and used as HttpContext.TraceIdentifier so it
// surfaces as traceId in ProblemDetails.
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    // Bound a client-supplied id so it fits every downstream store (e.g. audit_log).
    public const int MaxLength = 64;

    public async Task Invoke(HttpContext context)
    {
        var id = context.Request.Headers.TryGetValue(HeaderName, out var supplied)
                 && !string.IsNullOrWhiteSpace(supplied)
            ? supplied.ToString()
            : Guid.NewGuid().ToString("n");

        if (id.Length > MaxLength)
            id = id[..MaxLength];

        context.TraceIdentifier = id;
        context.Response.Headers[HeaderName] = id;

        using (LogContext.PushProperty("CorrelationId", id))
            await next(context);
    }
}
