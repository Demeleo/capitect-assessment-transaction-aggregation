using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TransactionAggregator.Application.Exceptions;
using TransactionAggregator.Application.Interfaces;
using TransactionAggregator.Application.Telemetry;
using TransactionAggregator.Domain.Interfaces;

namespace TransactionAggregator.Application.Services
{
	public class TransactionsRetrievalService : ITransactionsRetrievalService
	{
		private readonly IEnumerable<IFinancialVendor> _vendors;
		private readonly ITransactionRepository _transactionRepository;
		private readonly ILogger<TransactionsRetrievalService> _logger;
		public TransactionsRetrievalService(IEnumerable<IFinancialVendor> vendors, ITransactionRepository transactionRepository, ILogger<TransactionsRetrievalService> logger)
		{
			_vendors = vendors;
			_transactionRepository = transactionRepository;
			_logger = logger;
		}
		public async Task RetrieveAllTransactionDataFromVendors(CancellationToken cancellationToken = default)
		{
			using var activity = ApplicationTelemetry.ActivitySource.StartActivity("RetrieveAllTransactionDataFromVendors", ActivityKind.Internal);
			activity?.SetTag("vendor.count", _vendors.Count());

			IEnumerable<TransactionAggregator.Domain.Entities.Transaction>[] results;
			try
			{
				var tasks = _vendors.Select(v => v.GetTransactionsAsync(cancellationToken));
				results = await Task.WhenAll(tasks);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error while retrieving transactions from vendors");
				throw new AggregatorServiceException("Failed to retrieve transactions from one or more vendors.", ex);
			}

			var allTransactions = results.SelectMany(x => x);

			if (allTransactions.Any())
			{
				var uniqueResults = allTransactions
					.GroupBy(t => new { t.Vendor, t.AccountId, t.ProccesedDateTime, t.Amount })
					.Select(g => g.FirstOrDefault());

				var transactionCount = uniqueResults.Count();
				activity?.SetTag("transactions.count", transactionCount);

				if (uniqueResults.Any())
				{
					_logger.LogInformation("Save {Count} transactions to database", transactionCount);
					await _transactionRepository.AddAllAsync(uniqueResults!, cancellationToken);
				}
			}
			else
			{
				_logger.LogInformation("No transactions were returned from vendors.");
			}

		}
	}
}
