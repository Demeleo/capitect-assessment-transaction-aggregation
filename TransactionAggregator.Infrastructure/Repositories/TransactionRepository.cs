using Microsoft.EntityFrameworkCore;
using TransactionAggregator.Application.DTOs;
using TransactionAggregator.Application.Interfaces;
using TransactionAggregator.Domain.Entities;
using TransactionAggregator.Infrastructure.Data;
using TransactionAggregator.Infrastructure.Entities;

namespace TransactionAggregator.Infrastructure.Repositories
{
	public class TransactionRepository : ITransactionRepository
	{
		private readonly AppDbContext _db;

		public TransactionRepository(AppDbContext db)
		{
			_db = db;
		}

		public async Task<IEnumerable<TransactionAggregatedByCustomer>> GetAggregatedByCustomerAsync(CancellationToken cancellationToken)
		{
			var transactions = await _db.Transactions
				.Include(t => t.Account)
				.ToListAsync(cancellationToken);

			return transactions
				.GroupBy(t => t.Account.CustomerId)
				.Select(g => new TransactionAggregatedByCustomer
				{
					CustomerId = g.Key,
					TotalAmount = g.Sum(t => t.Amount),
					TransactionCount = g.Count(),
					CategoryTotalAmounts = g
								.GroupBy(t => t.Category)
								.ToDictionary(cta => cta.Key, cta => cta.Sum(t => t.Amount))
				})
				.ToList();

		}

		public async Task<IEnumerable<TransactionAggregatedByCategory>> GetAggregatedByCategoryAsync(CancellationToken cancellationToken)
		{
			var transactions = await _db.Transactions
				.Include(t => t.Account)
				.ToListAsync(cancellationToken);

			return transactions.GroupBy(t => t.Category)
					.Select(g => new TransactionAggregatedByCategory
					{
						Category = g.Key,
						TotalAmount = g.Sum(t => t.Amount),
						TransactionCount = g.Count(),
						AmountPerCustomer = g
									.GroupBy(t => t.Account.CustomerId)
									.ToDictionary(apc => apc.Key, apc => apc.Sum(t => t.Amount))
					})
					.ToList();
		}

		public async Task AddAllAsync(IEnumerable<Transaction> transactions, CancellationToken cancellationToken = default)
		{
			var existingCustomers = new HashSet<string>();
			var existingAccounts = new HashSet<string>();

			foreach (var t in transactions)
			{
				var customerId = t.CustomerId;
				if (!existingCustomers.Contains(customerId))
				{
					var customer = await _db.Customers
							.FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);

					if (customer == null)
					{
						customer = new CustomerEntity { Id = customerId };
						_db.Customers.Add(customer);
					}

					existingCustomers.Add(customerId);
				}

				var accountId = t.AccountId;
				if (!existingAccounts.Contains(accountId))
				{
					var account = await _db.Accounts
							.FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

					if (account == null)
					{
						account = new AccountEntity
						{
							Id = accountId,
							Vendor = t.Vendor,
							CustomerId = customerId
						};
						_db.Accounts.Add(account);
					}

					existingAccounts.Add(accountId);
				}
			}

			await _db.SaveChangesAsync(cancellationToken);


			var entities = transactions.Select(t => new TransactionEntity
			{
				AccountId = t.AccountId,
				Vendor = t.Vendor,
				ProccesedDateTime = t.ProccesedDateTime,
				Amount = t.Amount,
				Merchant = t.Merchant,
				Category = t.Category
			});

			var existingKeys = await _db.Transactions
						.Select(tx => new { tx.AccountId, tx.ProccesedDateTime, tx.Amount, tx.Merchant, tx.Vendor })
						.ToListAsync(cancellationToken);

			var newTransactions = entities
					.Where(e => !existingKeys.Contains(new { e.AccountId, e.ProccesedDateTime, e.Amount, e.Merchant, e.Vendor }))
					.ToList();

			if (newTransactions.Any())
			{
				_db.Transactions.AddRange(newTransactions);
				await _db.SaveChangesAsync(cancellationToken);
			}
		}

		public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime? processedFrom, DateTime? processedTo, CancellationToken cancellationToken)
		{
			var query = _db.Transactions
				.Include(t => t.Account)
				.AsNoTracking();

			if (processedFrom.HasValue)
				query = query.Where(t => t.ProccesedDateTime >= processedFrom.Value);

			if (processedTo.HasValue)
				query = query.Where(t => t.ProccesedDateTime <= processedTo.Value);

			return await query
					.Select(t => new Transaction
					{
						CustomerId = t.Account.CustomerId,
						AccountId = t.AccountId,
						Vendor = t.Vendor,
						ProccesedDateTime = t.ProccesedDateTime,
						Amount = t.Amount,
						Merchant = t.Merchant,
						Category = t.Category
					})
					.ToListAsync(cancellationToken);

		}
	}
}
