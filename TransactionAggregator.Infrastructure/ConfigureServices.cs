using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using TransactionAggregator.Application.Interfaces;
using TransactionAggregator.Domain.Interfaces;
using TransactionAggregator.Infrastructure.Caching;
using TransactionAggregator.Infrastructure.Configuration;
using TransactionAggregator.Infrastructure.Data;
using TransactionAggregator.Infrastructure.Factories;
using TransactionAggregator.Infrastructure.Repositories;
using TransactionAggregator.Infrastructure.Vendors;
using TransactionAggregator.Infrastructure.Vendors.DataGenerator;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace TransactionAggregator.Infrastructure
{
	public static class ConfigureServices
	{
		public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
		{
			services.AddOpenTelemetry()
				.WithTracing(tracing =>
				{
					tracing.AddSqlClientInstrumentation(options =>
					{
						options.EnrichWithSqlCommand = (activity, command) =>
						{
							if (command is System.Data.Common.DbCommand dbCommand)
							{
								activity.SetTag("db.statement", dbCommand.CommandText);
							}
						};
						options.RecordException = true;
					});

					tracing.AddHttpClientInstrumentation(options =>
					{
						options.RecordException = true;
					});
				})
				.WithMetrics(metrics =>
				{
					metrics.AddRuntimeInstrumentation();
					metrics.AddHttpClientInstrumentation();
				});

			services.AddHybridCache();
			services.AddSingleton<ICacheService, CacheService>();

			services.AddDbContext<AppDbContext>(options =>
						options.UseSqlServer(connectionString));

			services.AddScoped<ICustomerRepository, CustomerRepository>();
			services.AddScoped<ITransactionRepository, TransactionRepository>();

			services.AddHttpClient("VendorA", (sp, client) =>
			{
				var settings = sp.GetRequiredService<IOptions<VendorSettings>>().Value;
				client.BaseAddress = new Uri(settings.VendorA.BaseUrl);
				client.Timeout = TimeSpan.FromSeconds(settings.DefaultTimeoutSeconds);
			})
			.AddPolicyHandler((sp, req) =>
			{
				var settings = sp.GetRequiredService<IOptions<VendorSettings>>().Value;
				var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("VendorB.Policy");
				return PolicyFactory.GetRetryPolicy(logger, settings.DefaultRetryCount > 0 ? settings.DefaultRetryCount : 3);
			})
			.AddPolicyHandler((sp, req) =>
			{
				var settings = sp.GetRequiredService<IOptions<VendorSettings>>().Value;
				var timeout = TimeSpan.FromSeconds(settings.DefaultTimeoutSeconds > 0 ? settings.DefaultTimeoutSeconds : 10);
				return PolicyFactory.GetTimeoutPolicy(timeout);
			}); 

			services.AddHttpClient("VendorB", (sp, client) =>
			{
				var settings = sp.GetRequiredService<IOptions<VendorSettings>>().Value;
				client.BaseAddress = new Uri(settings.VendorB.BaseUrl);
				client.Timeout = TimeSpan.FromSeconds(settings.DefaultTimeoutSeconds);
			})
			.AddPolicyHandler((sp, req) =>
			{
				var settings = sp.GetRequiredService<IOptions<VendorSettings>>().Value;
				var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("VendorB.Policy");
				return PolicyFactory.GetRetryPolicy(logger, settings.DefaultRetryCount > 0 ? settings.DefaultRetryCount : 3);
			})
			.AddPolicyHandler((sp, req) =>
			{
				var settings = sp.GetRequiredService<IOptions<VendorSettings>>().Value;
				var timeout = TimeSpan.FromSeconds(settings.DefaultTimeoutSeconds > 0 ? settings.DefaultTimeoutSeconds : 10);
				return PolicyFactory.GetTimeoutPolicy(timeout);
			});


			services.AddSingleton<IFinancialVendor, VendorA>();
			services.AddSingleton<IFinancialVendor, VendorB>();

			//add server simulation for Vendors in order to use httpclient.
			var vendorAServer = WireMockServer.Start(9000);
			var vendorBServer = WireMockServer.Start(9001);

			vendorAServer.Given(
				Request.Create().WithPath("/transactions").UsingGet()
			).RespondWith(
				Response.Create()
				.WithDelay(TimeSpan.FromMilliseconds(300))
				.WithStatusCode(200)
				.WithHeader("Content-Type", "application/json")
				.WithBodyAsJson(DataGenerator.Generate(12, "Vendor A"))
			);

			vendorBServer.Given(
				Request.Create().WithPath("/transactions").UsingGet()
			).RespondWith(
				Response.Create()
				.WithDelay(TimeSpan.FromMilliseconds(300))
				.WithStatusCode(200)
				.WithHeader("Content-Type", "application/json")
				.WithBodyAsJson(DataGenerator.Generate(10, "Vendor B"))
			);

			return services;
		}
	}
}
