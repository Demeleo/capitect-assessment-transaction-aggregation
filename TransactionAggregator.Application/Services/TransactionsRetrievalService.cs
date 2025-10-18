using TransactionAggregator.Application.Interfaces;
using TransactionAggregator.Domain.Interfaces;

namespace TransactionAggregator.Application.Services
{
	public class TransactionsRetrievalService : ITransactionsRetrievalService
	{
		private readonly IEnumerable<IFinancialVendor> _vendors;
		private readonly ITransactionRepository _transactionRepository;
		public TransactionsRetrievalService(IEnumerable<IFinancialVendor> vendors, ITransactionRepository transactionRepository)
		{
			_vendors = vendors;
			_transactionRepository = transactionRepository;
		}
		public async Task RetrieveAllTransactionDataFromVendors(CancellationToken cancellationToken = default)
		{
			var tasks = _vendors.Select(v => v.GetTransactionsAsync(cancellationToken));
			var results = await Task.WhenAll(tasks);

			var allTransactions = results.SelectMany(x => x);

			if (allTransactions.Any())
			{
				var uniqueResults = allTransactions
					.GroupBy(t => new { t.Vendor, t.AccountId, t.ProccesedDateTime, t.Amount })
					.Select(g => g.FirstOrDefault());

				if (uniqueResults.Any()) await _transactionRepository.AddAllAsync(uniqueResults!, cancellationToken);
			}
		}
	}
}
