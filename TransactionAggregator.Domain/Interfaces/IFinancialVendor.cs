

using TransactionAggregator.Domain.Entities;

namespace TransactionAggregator.Domain.Interfaces
{
	public interface IFinancialVendor
	{
		string Name { get; }

		Task<IEnumerable<Transaction>> GetTransactionsAsync(CancellationToken cancellationToken = default);
	}
}
