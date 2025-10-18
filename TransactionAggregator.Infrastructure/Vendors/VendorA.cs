using System.Threading;
using TransactionAggregator.Application.DTOs;
using TransactionAggregator.Domain.Entities;
using TransactionAggregator.Domain.Interfaces;
using TransactionAggregator.Infrastructure.Vendors.DataGenerator;

namespace TransactionAggregator.Infrastructure.Vendors
{
	public class VendorA : IFinancialVendor
	{
		private readonly GenerateCustomers _customers;
		public string Name => "Vendor A";

		public VendorA(GenerateCustomers customers)
		{
			_customers = customers;
		}

		public async Task<IEnumerable<Transaction>> GetTransactionsAsync(CancellationToken cancellationToken = default)
		{
			await Task.Delay(200, cancellationToken);

			var customers = _customers.GetCustomers();

			var transactions = customers.SelectMany(c =>
					GenerateTransactions.Generate(Name, c, Random.Shared.Next(1, 10)));

			return transactions;
		}
	}
}
