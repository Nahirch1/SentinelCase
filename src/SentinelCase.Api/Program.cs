using System.Threading.RateLimiting;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using SentinelCase.Api.Common.Authorization;
using SentinelCase.Api.Common.ExceptionHandling;
using SentinelCase.Api.Common.Identity;
using SentinelCase.Api.Endpoints;
using SentinelCase.Application;
using SentinelCase.Application.Common.Interfaces;
using SentinelCase.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((services, configuration) =>
    configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override(
            "Microsoft.AspNetCore",
            LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console(
            outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] " +
                "[TraceId:{TraceId}] [User:{User}] " +
                "{Message:lj}{NewLine}{Exception}"));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<
        SentinelCase.Infrastructure.Persistence.ApplicationDbContext>(
        "database");

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(
            httpContext =>
            {
                var partitionKey =
                    httpContext.Connection.RemoteIpAddress?
                        .ToString()
                    ?? "unknown";

                return RateLimitPartition
                    .GetFixedWindowLimiter(
                        partitionKey,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        });
            });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy(
        AppPolicies.CanCreateIncident,
        policy => policy.RequireRole(
            AppRoles.Analyst,
            AppRoles.SocManager,
            AppRoles.Administrator))
    .AddPolicy(
        AppPolicies.CanManageIncidentStatus,
        policy => policy.RequireRole(
            AppRoles.SocManager,
            AppRoles.Administrator))
    .AddPolicy(
        AppPolicies.CanAssignIncident,
        policy => policy.RequireRole(
            AppRoles.SocManager,
            AppRoles.Administrator));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (
        diagnosticContext,
        httpContext) =>
    {
        diagnosticContext.Set(
            "TraceId",
            httpContext.TraceIdentifier);

        diagnosticContext.Set(
            "User",
            httpContext.User.Identity?.Name
                ?? "anonymous");
    };
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks(
    "/health",
    new HealthCheckOptions
    {
        Predicate = _ => false
    });

app.MapHealthChecks(
    "/health/ready");

app.MapControllers();
app.MapIncidentEndpoints();

app.Run();

public partial class Program;
