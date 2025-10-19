using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TransactionAggregator.Api.Middleware;
using TransactionAggregator.Api.Workers;
using TransactionAggregator.Application;
using TransactionAggregator.Infrastructure;
using TransactionAggregator.Infrastructure.Configuration;
using TransactionAggregator.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

var serviceName = "TransactionAggregator.Api";
var serviceVersion = "1.0.0.0";

var otlpEndpoint = builder.Configuration.GetValue<string>("Telemetry:OtlpEndpoint");
var isDevelopment = builder.Environment.IsDevelopment();

builder.Services.AddOpenTelemetry()
		.ConfigureResource(rb => rb.AddService(serviceName, serviceVersion: serviceVersion)
			.AddTelemetrySdk()
			.AddEnvironmentVariableDetector())
		.WithTracing(t =>
		{
			t.AddAspNetCoreInstrumentation()
			 .AddHttpClientInstrumentation()
			 .AddSqlClientInstrumentation();
			if (isDevelopment || string.IsNullOrWhiteSpace(otlpEndpoint))
			{
				t.AddConsoleExporter();
			}
			else
			{
				t.AddOtlpExporter(options =>
				{
					options.Endpoint = new Uri(otlpEndpoint);
				});
			}
		})
		.WithMetrics(m =>
		{
			m.AddAspNetCoreInstrumentation()
			 .AddHttpClientInstrumentation()
			 .AddRuntimeInstrumentation();
			if (isDevelopment || string.IsNullOrWhiteSpace(otlpEndpoint))
			{
				m.AddConsoleExporter();
			}
			else
			{
				m.AddOtlpExporter(options =>
				{
					options.Endpoint = new Uri(otlpEndpoint);
				});
			}
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


	if (isDevelopment || string.IsNullOrWhiteSpace(otlpEndpoint))
	{
		logBuilder.AddConsoleExporter();
	}
	else
	{
		logBuilder.AddOtlpExporter(options =>
		{
			options.Endpoint = new Uri(otlpEndpoint);
		});
	}
});

builder.Services.Configure<VendorSettings>(
		builder.Configuration.GetSection("Vendors"));

builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);
builder.Services.AddApplicationServices();
builder.Services.AddHostedService<TransactionDataWorker>();
builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

//typically this would be enabled only in development environment
//for the purpose of this assessment we enable it always
//if (app.Environment.IsDevelopment())
//{
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	db.Database.Migrate();
}
if (app.Environment.IsDevelopment())
{
	app.Urls.Add("https://0.0.0.0:7297");
	app.Urls.Add("http://0.0.0.0:5280");
}
else
	app.Urls.Add("http://0.0.0.0:5000");
app.Run();
