using TransactionAggregator.Application.DTOs;
using TransactionAggregator.Domain.Entities;

namespace TransactionAggregator.Application.Interfaces
{
	public interface ITransactionRepository
	{
		Task<IEnumerable<TransactionAggregatedByCustomer>> GetAggregatedByCustomerAsync(CancellationToken cancellationToken);
		Task<IEnumerable<TransactionAggregatedByCategory>> GetAggregatedByCategoryAsync(CancellationToken cancellationToken);
		IQueryable<Transaction> Query();
		Task AddAllAsync(IEnumerable<Transaction> transactions, CancellationToken cancellationToken = default);
		Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime? processedFrom, DateTime? processedTo, CancellationToken cancellationToken);
	}
}
