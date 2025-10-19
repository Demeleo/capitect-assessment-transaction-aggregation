using Microsoft.Extensions.DependencyInjection;
using TransactionAggregator.Application.Interfaces;
using TransactionAggregator.Application.Services;

namespace TransactionAggregator.Application
{
	public static class ConfigureServices
	{
		public static IServiceCollection AddApplicationServices(this IServiceCollection services)
		{
			services.AddScoped<ITransactionsRetrievalService, TransactionsRetrievalService>();
			services.AddScoped<ICustomerService, CustomerService>();
			services.AddScoped<ITransactionService, TransactionService>();
			services.AddScoped<ITransactionAggregationService, TransactionAggregationService>();
			return services;
		}
	}
}
