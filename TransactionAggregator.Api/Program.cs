using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TransactionAggregator.Api.Workers;
using TransactionAggregator.Application;
using TransactionAggregator.Infrastructure;
using TransactionAggregator.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

var serviceName = "TransactionAggregator.Api";
var serviceVersion = "1.0.0.0";

builder.Services.AddOpenTelemetry()
		.ConfigureResource(rb => rb.AddService(serviceName, serviceVersion: serviceVersion)
			.AddTelemetrySdk()
			.AddEnvironmentVariableDetector())
		.WithTracing(t =>
		{
			t.AddAspNetCoreInstrumentation()
			 .AddHttpClientInstrumentation()
			 .AddSqlClientInstrumentation()
			 .AddConsoleExporter();
		})
		.WithMetrics(m =>
		{
			m.AddAspNetCoreInstrumentation()
			 .AddHttpClientInstrumentation()
			 .AddRuntimeInstrumentation()
			 .AddConsoleExporter();
		});

builder.Logging.ClearProviders(); // optional: remove default Console/EventSource providers

builder.Logging.AddOpenTelemetry(logBuilder =>
{
	logBuilder
				.SetResourceBuilder(
					ResourceBuilder.CreateDefault()
						.AddService(serviceName, serviceVersion: serviceVersion)
						.AddTelemetrySdk()
						.AddEnvironmentVariableDetector());

	logBuilder.IncludeScopes = true;
	logBuilder.IncludeFormattedMessage = true;
	logBuilder.ParseStateValues = true;

	// Export to console for now
	logBuilder.AddConsoleExporter();

	// For production, you could add an OTLP exporter (e.g. to Grafana or Jaeger):
	// logBuilder.AddOtlpExporter(options =>
	// {
	//     options.Endpoint = new Uri("http://otel-collector:4317");
	// });
});

builder.Services.AddInfrastructureServices(builder.Configuration.GetConnectionString("DefaultConnection")!);
builder.Services.AddApplicationServices();
builder.Services.AddHostedService<TransactionDataGenerator>();
builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	db.Database.Migrate();
}

app.Run();
