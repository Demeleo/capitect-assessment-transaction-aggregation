using TransactionAggregator.Application.DTOs;
using TransactionAggregator.Application.Interfaces;
using TransactionAggregator.Application.Requests;

namespace TransactionAggregator.Application.Services
{
	public class TransactionService : ITransactionService
	{
		private readonly ITransactionRepository _transactionRepository;
		private readonly ICacheService _cacheService;
		public TransactionService(ITransactionRepository transactionRepository, ICacheService cacheService)
		{
			_transactionRepository = transactionRepository;
			_cacheService = cacheService;
		}

		public async Task<IEnumerable<TransactionDto>> GetFilteredTransactionsAsync(TransactionQueryParameters filters, CancellationToken cancellationToken)
		{
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

			if (cacheKey is not null)
			{
				return await _cacheService.ExecuteWithCache(cacheKey, GetDataAsync, expiration);
			}

			return await GetDataAsync();

		}
	}
}
