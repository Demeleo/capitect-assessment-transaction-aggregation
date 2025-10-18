
namespace TransactionAggregator.Application.Interfaces
{
	public interface ITransactionsRetrievalService
	{
		Task RetrieveAllTransactionDataFromVendors(CancellationToken cancellationToken = default);
	}
}
