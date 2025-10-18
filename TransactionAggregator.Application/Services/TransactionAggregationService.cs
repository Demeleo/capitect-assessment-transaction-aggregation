using TransactionAggregator.Application.DTOs;
using TransactionAggregator.Application.Interfaces;

namespace TransactionAggregator.Application.Services
{
	public class TransactionAggregationService : ITransactionAggregationService
	{
		private readonly ITransactionRepository _transactionRepository;
		public TransactionAggregationService(ITransactionRepository transactionRepository)
		{
			_transactionRepository = transactionRepository;
		}
		public async Task<IEnumerable<TransactionAggregatedByCategoryDto>> GetAggregatedByCategoryAsync(CancellationToken cancellationToken)
		{
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
