using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TransactionAggregator.Application.DTOs;
using TransactionAggregator.Application.Interfaces;
using TransactionAggregator.Application.Telemetry;

namespace TransactionAggregator.Application.Services
{
	public class TransactionAggregationService : ITransactionAggregationService
	{
		private readonly ITransactionRepository _transactionRepository;
		private readonly ILogger<TransactionAggregationService> _logger;
		public TransactionAggregationService(ITransactionRepository transactionRepository, ILogger<TransactionAggregationService> logger)
		{
			_transactionRepository = transactionRepository;
			_logger = logger;	
		}
		public async Task<IEnumerable<TransactionAggregatedByCategoryDto>> GetAggregatedByCategoryAsync(CancellationToken cancellationToken)
		{
			using var activity = ApplicationTelemetry.ActivitySource.StartActivity("GetAggregatedByCategory", ActivityKind.Server);
			_logger.LogInformation("Get aggregated transasctions by category.");

			var aggregatedByCustomerResult = await _transactionRepository.GetAggregatedByCategoryAsync(cancellationToken);
			return aggregatedByCustomerResult.Select(a => new TransactionAggregatedByCategoryDto
			{
				Category = a.Category,
				TotalAmount = a.TotalAmount,
				TransactionCount = a.TransactionCount,
				AmountPerCustomer = a.AmountPerCustomer
			});

		}

		public async Task<IEnumerable<TransactionAggregatedByCustomerDto>> GetAggregatedByCustomerAsync(CancellationToken cancellationToken)
		{
			using var activity = ApplicationTelemetry.ActivitySource.StartActivity("GetAggregatedByCustomer", ActivityKind.Server);
			_logger.LogInformation("Get aggregated transasctions by customer.");

			var aggregatedByCustomer = await _transactionRepository.GetAggregatedByCustomerAsync(cancellationToken);
			return aggregatedByCustomer.Select(a => new TransactionAggregatedByCustomerDto
			{
				CustomerId = a.CustomerId,
				TotalAmount = a.TotalAmount,
				TransactionCount = a.TransactionCount,
				CategoryTotalAmounts = a.CategoryTotalAmounts
			});

		}
	}
}
