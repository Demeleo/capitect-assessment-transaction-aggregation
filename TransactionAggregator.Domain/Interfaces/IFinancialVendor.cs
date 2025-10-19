

using TransactionAggregator.Domain.Entities;

namespace TransactionAggregator.Domain.Interfaces
{
	public interface IFinancialVendor
	{

		Task<IEnumerable<Transaction>> GetTransactionsAsync(CancellationToken cancellationToken = default);
	}
}
