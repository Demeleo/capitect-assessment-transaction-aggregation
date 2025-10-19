using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAggregator.Domain.Entities;

namespace TransactionAggregator.Infrastructure.Vendors.DataGenerator
{
	public static class DataGenerator
	{
		public static IEnumerable<Transaction> Generate(int customerCount, string vendorName)
		{
			List<CustomerModel> customers = [];

			Randomizer.Seed = new Random(12345);
			var faker = new Faker<CustomerModel>()
					.RuleFor(c => c.Id, f => f.Random.Guid().ToString())
					.RuleFor(c => c.Name, f => f.Person.FullName)
					.RuleFor(c => c.Email, f => f.Internet.Email());

			customers.AddRange(faker.Generate(customerCount));

			var transactions = customers.SelectMany(c =>
					GenerateTransactions.Generate(vendorName, c, Random.Shared.Next(1, 10)));

			return transactions;
		}
	}
}
