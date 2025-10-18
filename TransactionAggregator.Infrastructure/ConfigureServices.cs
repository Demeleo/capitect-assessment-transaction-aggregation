using Microsoft.Extensions.DependencyInjection;
using TransactionAggregator.Application.Interfaces;
using TransactionAggregator.Infrastructure.Data;
using TransactionAggregator.Infrastructure.Repositories;
using TransactionAggregator.Infrastructure.Vendors;
using TransactionAggregator.Infrastructure.Vendors.DataGenerator;
using Microsoft.EntityFrameworkCore;
using TransactionAggregator.Domain.Interfaces;
using TransactionAggregator.Infrastructure.Caching;

namespace TransactionAggregator.Infrastructure
{
	public static class ConfigureServices
	{
		public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
		{
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
