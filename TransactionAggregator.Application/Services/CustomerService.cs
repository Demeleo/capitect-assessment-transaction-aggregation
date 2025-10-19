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
	public class CustomerService : ICustomerService
	{
		private readonly ICustomerRepository _customerRepository;
		private readonly ICacheService _cacheService;
		private readonly ILogger<CustomerService> _logger;
		public CustomerService(ICustomerRepository customerRepository, ICacheService cacheService, ILogger<CustomerService> logger)
		{
			_customerRepository = customerRepository;
			_cacheService = cacheService;
			_logger = logger;
		}

		public async Task<IEnumerable<TransactionDto>> GetTransactions(string customerId, CustomerQueryParameters filters, CancellationToken cancellationToken)
		{
			using var activity = ApplicationTelemetry.ActivitySource.StartActivity("GetCustomerTransactions", ActivityKind.Server);
			activity?.SetTag("filter", filters);

			var errors = new Dictionary<string, string[]>();
			if (filters.Page < 1) errors["Page"] = ["Page must be at least 1."];
			if (filters.PageSize < 2 || filters.PageSize > 500) errors["PageSize"] = ["PageSize must be between 2 and 500."];

			if (errors.Count > 0)
			{
				_logger.LogWarning("Validation failed. Invalid query parameters.");
				throw new AggregatorValidationException("Invalid query parameters.", errors);
			}

			var cacheKey = filters.GetCacheKey();
			var expiration = TimeSpan.FromMinutes(30);

			async Task<IEnumerable<TransactionDto>> GetDataAsync()
			{
				var transactions = await _customerRepository.GetTransactionsByDateRangeAsync(customerId, filters.ProcessedFrom, filters.ProcessedTo, cancellationToken);

				if (!string.IsNullOrEmpty(filters.AccountId))
					transactions = transactions.Where(t => t.AccountId == filters.AccountId);

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
