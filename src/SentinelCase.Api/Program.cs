using Microsoft.AspNetCore.Authentication.JwtBearer;

using SentinelCase.Api.Common.Authorization;
using SentinelCase.Api.Common.ExceptionHandling;
using SentinelCase.Api.Endpoints;
using SentinelCase.Application;
using SentinelCase.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

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
            AppRoles.Administrator));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapIncidentEndpoints();

app.Run();

public partial class Program;
