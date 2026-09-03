using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SupportPlatform.Application.Common;
using SupportPlatform.Application.Search;

namespace SupportPlatform.Api.Errors;

/// <summary>
/// Turns an unhandled exception into an RFC 7807 response (<c>docs/contracts/error-model.md</c>).
/// FluentValidation failures and the Application <see cref="InvalidQueryException"/> become 400
/// <c>validation</c>; <see cref="ForbiddenException"/> is 403, <see cref="NotFoundException"/> is
/// 404; anything else is a logged 500 <c>unexpected</c>.
/// </summary>
public sealed class ExceptionToProblemDetailsHandler(ILogger<ExceptionToProblemDetailsHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception exception, CancellationToken ct)
    {
        var (status, detail, errors) = Map(exception);

        if (status == 500)
            logger.LogError(exception, "Unhandled exception. traceId={TraceId}", ctx.TraceIdentifier);

        var (type, title) = ProblemTypes.ForStatus(status);
        var problem = new ProblemDetails
        {
            Status = status,
            Type = type,
            Title = title,
            Detail = detail
        };
        problem.Extensions["traceId"] = ctx.TraceIdentifier;
        if (errors is not null)
            problem.Extensions["errors"] = errors;

        ctx.Response.StatusCode = status;
        await ctx.Response.WriteAsJsonAsync(
            problem, (JsonSerializerOptions?)null, "application/problem+json", ct);
        return true;
    }

    private static (int Status, string Detail, IDictionary<string, string[]>? Errors) Map(Exception ex) => ex switch
    {
        ValidationException v => (
            400,
            "The query definition failed validation.",
            v.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray())),

        InvalidQueryException q => (
            400,
            "The query definition failed validation.",
            new Dictionary<string, string[]> { [q.Field] = [q.Message] }),

        ForbiddenException f => (403, f.Message, null),

        NotFoundException n => (404, n.Message, null),

        _ => (500, "The request could not be completed. Reference traceId when reporting.", null)
    };
}
