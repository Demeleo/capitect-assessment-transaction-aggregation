using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TransactionAggregator.Application.Requests
{
	public class CustomerQueryParameters
	{
		public string? AccountId { get; set; }
		public DateTime? ProcessedFrom { get; set; }
		public DateTime? ProcessedTo { get; set; }
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 50;

		public string? GetCacheKey()
		{
			if (ProcessedTo != null)
			{
				byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this)));
				return Convert.ToBase64String(bytes);
			}

			return null;
		}
	}
}
