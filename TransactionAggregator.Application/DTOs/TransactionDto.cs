
namespace TransactionAggregator.Application.DTOs
{
	public class TransactionDto
	{
		public string Vendor { get; set; } = string.Empty;
		public string AccountId { get; set; } = string.Empty;
		public string CustomerId { get; set; } = string.Empty;
		public decimal Amount { get; set; }
		public DateTime ProccesedDateTime { get; set; }
		public string Merchant { get; set; } = string.Empty;
		public string Category { get; set; } = string.Empty;
	}
}
