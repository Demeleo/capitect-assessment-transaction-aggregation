using TransactionAggregator.Application.DTOs;

namespace TransactionAggregator.Application.Interfaces
{
	public interface ITransactionAggregationService
	{
		Task<IEnumerable<TransactionAggregatedByCustomerDto>> GetAggregatedByCustomerAsync(CancellationToken cancellationToken);
		Task<IEnumerable<TransactionAggregatedByCategoryDto>> GetAggregatedByCategoryAsync(CancellationToken cancellationToken);

	}
}
