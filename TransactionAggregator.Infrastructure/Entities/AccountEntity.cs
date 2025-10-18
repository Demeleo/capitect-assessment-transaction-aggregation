namespace TransactionAggregator.Infrastructure.Entities
{
	public class AccountEntity
	{
		public string Id { get; set; } = string.Empty;
		public string Vendor { get; set; } = string.Empty;
		public string AccountNumber { get; set; } = string.Empty;
		public string CustomerId { get; set; } = string.Empty;
		public CustomerEntity Customer { get; set; }
		public ICollection<TransactionEntity> Transactions { get; set; } = [];
	}
}
