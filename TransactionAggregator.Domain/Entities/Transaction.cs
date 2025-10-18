namespace TransactionAggregator.Domain.Entities
{
	public class Transaction
	{
		public string Id { get; set; }
		public string AccountId { get; set; }
		public string CustomerId { get; set; }
		public string Vendor { get; set; }
		public string Merchant { get; set; }
		public string Category { get; set; }
		public decimal Amount { get; set; }
		public DateTime ProccesedDateTime { get; set; }

		public string DisplaySummary()
		{
			return $"{Merchant} ({Category}): {Amount:C} on {ProccesedDateTime:d}";
		}
	}
}
