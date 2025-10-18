namespace TransactionAggregator.Infrastructure.Entities
{
	public class TransactionEntity
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public string AccountId { get; set; } = string.Empty;
		public AccountEntity Account { get; set; }
		public string Vendor { get; set; } = default!;
		public DateTime ProccesedDateTime { get; set; }
		public decimal Amount { get; set; }
		public string Merchant { get; set; } = default!;
		public string Category { get; set; } = default!;
	}
}
