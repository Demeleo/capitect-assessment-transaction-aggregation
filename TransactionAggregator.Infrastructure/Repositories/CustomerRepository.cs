using Microsoft.EntityFrameworkCore;
using TransactionAggregator.Application.DTOs;
using TransactionAggregator.Application.Interfaces;
using TransactionAggregator.Domain.Entities;
using TransactionAggregator.Infrastructure.Data;
using TransactionAggregator.Infrastructure.Entities;

namespace TransactionAggregator.Infrastructure.Repositories
{
	public class CustomerRepository : ICustomerRepository
	{
		private readonly AppDbContext _db;

		public CustomerRepository(AppDbContext db)
		{
			_db = db;
		}

		public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(string customerId, DateTime? processedFrom, DateTime? processedTo, CancellationToken cancellationToken)
		{
			var query = _db.Transactions
				.Include(t => t.Account)
				.AsNoTracking()
				.Where(t => t.Account.CustomerId == customerId); 

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
