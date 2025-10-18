using Microsoft.Extensions.DependencyInjection;
using TransactionAggregator.Application.Interfaces;
using TransactionAggregator.Infrastructure.Data;
using TransactionAggregator.Infrastructure.Repositories;
using TransactionAggregator.Infrastructure.Vendors;
using TransactionAggregator.Infrastructure.Vendors.DataGenerator;
using Microsoft.EntityFrameworkCore;
using TransactionAggregator.Domain.Interfaces;
using TransactionAggregator.Infrastructure.Caching;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

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

			services.AddScoped<ITransactionRepository, TransactionRepository>();

			services.AddSingleton<GenerateCustomers>();

			services.AddSingleton<IFinancialVendor, VendorA>();
			services.AddSingleton<IFinancialVendor, VendorB>();

			return services;
		}
	}
}
