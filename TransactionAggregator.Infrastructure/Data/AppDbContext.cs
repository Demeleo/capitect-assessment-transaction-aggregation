using Microsoft.EntityFrameworkCore;
using TransactionAggregator.Infrastructure.Entities;

namespace TransactionAggregator.Infrastructure.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<CustomerEntity> Customers { get; set; } = default!;
		public DbSet<AccountEntity> Accounts { get; set; } = default!;
		public DbSet<TransactionEntity> Transactions { get; set; } = default!;

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<AccountEntity>()
					.HasIndex(a => new { a.CustomerId, a.Vendor })
					.IsUnique();

			modelBuilder.Entity<TransactionEntity>()
					.HasIndex(t => new { t.AccountId, t.ProccesedDateTime, t.Amount, t.Merchant, t.Vendor })
					.IsUnique();

			modelBuilder.Entity<AccountEntity>()
					.HasOne(a => a.Customer)
					.WithMany(c => c.Accounts)
					.HasForeignKey(a => a.CustomerId);

			modelBuilder.Entity<TransactionEntity>()
					.HasOne(t => t.Account)
					.WithMany(a => a.Transactions)
					.HasForeignKey(t => t.AccountId);
		}
	}
}
