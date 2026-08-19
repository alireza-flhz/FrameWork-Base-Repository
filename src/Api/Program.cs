using BaseRepository.Api.ExceptionHandling;
using BaseRepository.Application;
using BaseRepository.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Ok(new { service = "BaseRepository.Api", status = "running" }));
app.MapControllers();

app.MapOpenApi();
app.MapScalarApiReference();

app.Run();

public partial class Program
{
}
