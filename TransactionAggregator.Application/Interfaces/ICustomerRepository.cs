using TransactionAggregator.Application.DTOs;
using TransactionAggregator.Domain.Entities;

namespace TransactionAggregator.Application.Interfaces
{
	public interface ICustomerRepository
	{
		Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(string suctomerId, DateTime? processedFrom, DateTime? processedTo, CancellationToken cancellationToken);
	}
}
