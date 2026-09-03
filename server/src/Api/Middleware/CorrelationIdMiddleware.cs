using Serilog.Context;

namespace SupportPlatform.Api.Middleware;

/// <summary>
/// Gives every request a correlation id: taken from the <c>X-Correlation-Id</c> request header or
/// generated. It is echoed on the response, pushed onto every Serilog line as
/// <c>CorrelationId</c>, and used as <c>HttpContext.TraceIdentifier</c> so it surfaces as
/// <c>traceId</c> in ProblemDetails (<c>docs/contracts/error-model.md</c>).
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    /// <summary>Bounds a client-supplied id so it fits every downstream store (e.g. <c>audit_log</c>).</summary>
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
