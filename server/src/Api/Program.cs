using Microsoft.EntityFrameworkCore;
using SupportPlatform.Application;
using SupportPlatform.Infrastructure;
using SupportPlatform.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

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
