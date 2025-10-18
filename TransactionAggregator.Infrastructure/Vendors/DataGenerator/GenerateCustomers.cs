using Bogus;

namespace TransactionAggregator.Infrastructure.Vendors.DataGenerator
{
	public class GenerateCustomers
	{
		private readonly List<CustomerModel> _customers = [];

		public GenerateCustomers(int customerCount = 10)
		{
			Generate(customerCount);
		}

		private void Generate(int count)
		{
			Randomizer.Seed = new Random(12345);
			var faker = new Faker<CustomerModel>()
					.RuleFor(c => c.Id, f => f.Random.Guid().ToString())
					.RuleFor(c => c.Name, f => f.Person.FullName)
					.RuleFor(c => c.Email, f => f.Internet.Email());

			_customers.AddRange(faker.Generate(count));
		}

		public IEnumerable<CustomerModel> GetCustomers() => _customers;
	}
}
