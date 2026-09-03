using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using SupportPlatform.Api.Errors;
using SupportPlatform.Api.Identity;
using SupportPlatform.Api.Middleware;
using SupportPlatform.Application;
using SupportPlatform.Application.Identity;
using SupportPlatform.Application.Search;
using SupportPlatform.Infrastructure;
using SupportPlatform.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <corr:{CorrelationId}>{NewLine}{Exception}"));

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new FilterValueJsonConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = ctx =>
{
    ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
    var (type, title) = ProblemTypes.ForStatus(ctx.ProblemDetails.Status ?? 500);
    ctx.ProblemDetails.Type ??= type;
    ctx.ProblemDetails.Title ??= title;
});
builder.Services.AddExceptionHandler<ExceptionToProblemDetailsHandler>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache(o => o.SizeLimit = 1000); // bound the search dedup cache (§ DESIGN_QA 5)
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddSingleton(new SearchCacheOptions
{
    TtlSeconds = builder.Configuration.GetValue("Search:CacheTtlSeconds", 60)
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<SupportPlatformDbContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
}

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

/// <summary>Exposed so the test host (<c>WebApplicationFactory</c>) can boot the API.</summary>
public partial class Program;
