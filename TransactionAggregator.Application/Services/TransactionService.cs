using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using TransactionAggregator.Application.DTOs;
using TransactionAggregator.Application.Exceptions;
using TransactionAggregator.Application.Interfaces;
using TransactionAggregator.Application.Requests;
using TransactionAggregator.Application.Telemetry;

namespace TransactionAggregator.Application.Services
{
	public class TransactionService : ITransactionService
	{
		private readonly ITransactionRepository _transactionRepository;
		private readonly ICacheService _cacheService;
		private readonly ILogger<TransactionService> _logger;
		public TransactionService(ITransactionRepository transactionRepository, ICacheService cacheService, ILogger<TransactionService> logger)
		{
			_transactionRepository = transactionRepository;
			_cacheService = cacheService;
			_logger = logger;
		}

		public async Task<IEnumerable<TransactionDto>> GetFilteredTransactionsAsync(TransactionQueryParameters filters, CancellationToken cancellationToken)
		{
			using var activity = ApplicationTelemetry.ActivitySource.StartActivity("GetFilteredTransactions", ActivityKind.Server);
			activity?.SetTag("filter", filters);

			var errors = new Dictionary<string, string[]>();
			if (filters.Page < 1) errors["Page"] = ["Page must be at least 1."];
			if (filters.PageSize < 1 || filters.PageSize > 500) errors["PageSize"] = ["PageSize must be between 1 and 500."];

			if (errors.Count > 0)
			{
				_logger.LogWarning("Validation failed. Invalid query parameters.");
				throw new AggregatorValidationException("Invalid query parameters.", errors);
			}

			var cacheKey = filters.GetCacheKey();
			var expiration = TimeSpan.FromMinutes(30);

			async Task<IEnumerable<TransactionDto>> GetDataAsync()
			{
				var transactions = await _transactionRepository.GetTransactionsByDateRangeAsync(filters.ProcessedFrom, filters.ProcessedTo, cancellationToken);

				if (!string.IsNullOrEmpty(filters.CustomerId))
					transactions = transactions.Where(t => t.CustomerId == filters.CustomerId);

				if (!string.IsNullOrEmpty(filters.AccountId))
					transactions = transactions.Where(t => t.AccountId == filters.AccountId);

				if (!string.IsNullOrEmpty(filters.Category))
					transactions = transactions.Where(t => t.Category == filters.Category);

				if (!string.IsNullOrEmpty(filters.Merchant))
					transactions = transactions.Where(t => t.Merchant == filters.Merchant);

				return transactions
						.Skip((filters.Page - 1) * filters.PageSize)
						.Take(filters.PageSize)
						.Select(t => new TransactionDto
						{
							AccountId = t.AccountId,
							CustomerId = t.CustomerId,
							Vendor = t.Vendor,
							ProccesedDateTime = t.ProccesedDateTime,
							Amount = t.Amount,
							Merchant = t.Merchant,
							Category = t.Category
						}).ToList();
			}

			if (cacheKey != null)
			{
				return await _cacheService.ExecuteWithCache(cacheKey, GetDataAsync, expiration);
			}

			return await GetDataAsync();

		}
	}
}
