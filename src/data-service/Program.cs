using DataService.Adapters;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Opentelemetry services to the container
// in PROD use ISTIO on K8S
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddOpenTelemetry()
       .WithMetrics(metrics =>
       {
           metrics.AddAspNetCoreInstrumentation();
           metrics.AddConsoleExporter();
       });
}

// Add My Services
builder.Services.AddApplicationServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
