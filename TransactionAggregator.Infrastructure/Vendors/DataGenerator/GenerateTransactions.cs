using Bogus;
using TransactionAggregator.Domain.Entities;

namespace TransactionAggregator.Infrastructure.Vendors.DataGenerator
{
	public class GenerateTransactions
	{
		private static readonly Dictionary<string, string[]> MerchantCategoryMap = new()
		{
			["Amazon"] = ["Shopping", "Electronics"],
			["Walmart"] = ["Groceries", "Shopping"],
			["Target"] = ["Groceries", "Shopping"],
			["Best Buy"] = ["Electronics", "Shopping"],
			["Netflix"] = ["Entertainment"],
			["Spotify"] = ["Entertainment"],
			["Uber"] = ["Transport", "Dining"],
			["Shell"] = ["Fuel"],
			["McDonald's"] = ["Dining", "Fast Food"],
			["Starbucks"] = ["Dining", "Coffee"]
		};

		public static IEnumerable<Transaction> Generate(
			string vendor,
			CustomerModel customer,
			int transactionCount = 25,
			DateTime? startDate = null)
		{
			var start = startDate ?? DateTime.UtcNow.AddDays(-30);

			var faker = new Faker<Transaction>()
				.RuleFor(t => t.Vendor, vendor)
				.RuleFor(t => t.AccountId, f => $"{vendor}-{customer.Id.ToString().Substring(0, 8)}")
				.RuleFor(t => t.CustomerId, customer.Id)
				.RuleFor(t => t.Merchant, f => f.PickRandom(MerchantCategoryMap.Keys.ToList()))
				.RuleFor(t => t.Amount, f => Math.Round(f.Finance.Amount(-200, 300), 2))
				.RuleFor(t => t.ProccesedDateTime, f => f.Date.Between(start, DateTime.UtcNow))
				.RuleFor(t => t.Category, (f, t) =>
				{
					// pick realistic category based on merchant
					var categories = MerchantCategoryMap[t.Merchant];
					return f.PickRandom(categories);
				});

			return faker.Generate(transactionCount);
		}
	}
}
