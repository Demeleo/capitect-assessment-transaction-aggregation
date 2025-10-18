using FluentAssertions;
using Moq;
using TransactionAggregator.Application.DTOs;
using TransactionAggregator.Application.Interfaces;
using TransactionAggregator.Application.Requests;
using TransactionAggregator.Application.Services;
using TransactionAggregator.Domain.Entities;

namespace TransactionAggregator.Tests;
[TestFixture]
public class TransactionServiceTests
{
	private Mock<ITransactionRepository> _repositoryMock;
	private Mock<ICacheService> _cacheMock;
	private TransactionService _service;

	[SetUp]
	public void SetUp()
	{
		_repositoryMock = new Mock<ITransactionRepository>();
		_cacheMock = new Mock<ICacheService>();
		_service = new TransactionService(_repositoryMock.Object, _cacheMock.Object);
	}

	[Test]
	public async Task Should_ReturnFilterAndPaginateCachedResult_WhenCacheKeyIsPresent()
	{
		var filters = new TransactionQueryParameters
		{
			ProcessedTo = DateTime.UtcNow,
			Page = 1,
			PageSize = 10
		};

		var cachedResult = new List<TransactionDto>
						{
								new TransactionDto { AccountId = "foo", CustomerId = "bar", Amount = 100 }
						};

		_cacheMock
				.Setup(c => c.ExecuteWithCache(
						It.IsAny<string>(),
						It.IsAny<Func<Task<IEnumerable<TransactionDto>>>>(),
						It.IsAny<TimeSpan>()))
				.ReturnsAsync(cachedResult);

		var result = await _service.GetFilteredTransactionsAsync(filters, CancellationToken.None);

		result.Should().BeEquivalentTo(cachedResult);
		_cacheMock.Verify(c => c.ExecuteWithCache(
				It.IsAny<string>(),
				It.IsAny<Func<Task<IEnumerable<TransactionDto>>>>(),
				It.IsAny<TimeSpan>()), Times.Once);

		_repositoryMock.Verify(r => r.GetTransactionsByDateRangeAsync(
				It.IsAny<DateTime?>(),
				It.IsAny<DateTime?>(),
				It.IsAny<CancellationToken>()), Times.Never);
	}

	[Test]
	public async Task Should_ReturnFilterAndPaginateNonCachedResult_WhenNoCacheKey()
	{
		var filters = new TransactionQueryParameters
		{
			CustomerId = "bar",
			Page = 1,
			PageSize = 2
		};

		var rawTransactions = new List<Transaction>
						{
								new Transaction { AccountId = "foo", CustomerId = "bar", Amount = 100 },
								new Transaction { AccountId = "alpha", CustomerId = "bar", Amount = 200 },
								new Transaction { AccountId = "delta", CustomerId = "omega", Amount = 300 }
						};

		_repositoryMock
				.Setup(r => r.GetTransactionsByDateRangeAsync(null, null, It.IsAny<CancellationToken>()))
				.ReturnsAsync(rawTransactions);

		var result = await _service.GetFilteredTransactionsAsync(filters, CancellationToken.None);

		result.Should().HaveCount(2);
		result.Should().OnlyContain(t => t.CustomerId == "bar");
		_cacheMock.Verify(c => c.ExecuteWithCache(
				It.IsAny<string>(),
				It.IsAny<Func<Task<IEnumerable<TransactionDto>>>>(),
				It.IsAny<TimeSpan>()), Times.Never);

		_repositoryMock.Verify(r => r.GetTransactionsByDateRangeAsync(
				It.IsAny<DateTime?>(),
				It.IsAny<DateTime?>(),
				It.IsAny<CancellationToken>()), Times.Once);
	}

	[Test]
	public async Task Should_ReturnReturnFilterAndPaginateOnAccountId_WhenAccountIdParameterIsSet()
	{
		var filters = new TransactionQueryParameters
		{
			AccountId = "alpha",
			Page = 1,
			PageSize = 2
		};

		var rawTransactions = new List<Transaction>
						{
								new Transaction { AccountId = "foo", CustomerId = "bar", Amount = 100 },
								new Transaction { AccountId = "alpha", CustomerId = "bar", Amount = 200 },
								new Transaction { AccountId = "delta", CustomerId = "omega", Amount = 300 }
						};

		_repositoryMock
				.Setup(r => r.GetTransactionsByDateRangeAsync(null, null, It.IsAny<CancellationToken>()))
				.ReturnsAsync(rawTransactions);

		var result = await _service.GetFilteredTransactionsAsync(filters, CancellationToken.None);

		result.Should().HaveCount(1);
		result.Should().OnlyContain(t => t.AccountId == "alpha");
		_cacheMock.Verify(c => c.ExecuteWithCache(
				It.IsAny<string>(),
				It.IsAny<Func<Task<IEnumerable<TransactionDto>>>>(),
				It.IsAny<TimeSpan>()), Times.Never);

		_repositoryMock.Verify(r => r.GetTransactionsByDateRangeAsync(
				It.IsAny<DateTime?>(),
				It.IsAny<DateTime?>(),
				It.IsAny<CancellationToken>()), Times.Once);
	}

	[Test]
	public async Task Should_ReturnReturnFilterAndPaginateOnCategory_WhenCategoryParameterIsSet()
	{
		var filters = new TransactionQueryParameters
		{
			Category = "shop",
			Page = 1,
			PageSize = 2
		};

		var rawTransactions = new List<Transaction>
						{
								new Transaction { AccountId = "foo", CustomerId = "bar", Amount = 100, Category = "shop" },
								new Transaction { AccountId = "alpha", CustomerId = "bar", Amount = 200, Category = "shop" },
								new Transaction { AccountId = "delta", CustomerId = "omega", Amount = 300, Category = "enter" },
								new Transaction { AccountId = "delta", CustomerId = "omega", Amount = 100, Category = "groc" }
						};

		_repositoryMock
				.Setup(r => r.GetTransactionsByDateRangeAsync(null, null, It.IsAny<CancellationToken>()))
				.ReturnsAsync(rawTransactions);

		var result = await _service.GetFilteredTransactionsAsync(filters, CancellationToken.None);

		result.Should().HaveCount(2);
		result.Should().OnlyContain(t => t.Category == "shop");
		_cacheMock.Verify(c => c.ExecuteWithCache(
				It.IsAny<string>(),
				It.IsAny<Func<Task<IEnumerable<TransactionDto>>>>(),
				It.IsAny<TimeSpan>()), Times.Never);

		_repositoryMock.Verify(r => r.GetTransactionsByDateRangeAsync(
				It.IsAny<DateTime?>(),
				It.IsAny<DateTime?>(),
				It.IsAny<CancellationToken>()), Times.Once);
	}

	[Test]
	public async Task Should_ReturnReturnFilterAndPaginateOnMerchant_WhenMerchantParameterIsSet()
	{
		var filters = new TransactionQueryParameters
		{
			Merchant = "shop",
			Page = 1,
			PageSize = 2
		};

		var rawTransactions = new List<Transaction>
						{
								new Transaction { AccountId = "foo", CustomerId = "bar", Amount = 100, Merchant = "shop" },
								new Transaction { AccountId = "alpha", CustomerId = "bar", Amount = 200, Merchant = "shop" },
								new Transaction { AccountId = "delta", CustomerId = "omega", Amount = 300, Merchant = "enter" },
								new Transaction { AccountId = "delta", CustomerId = "omega", Amount = 100, Merchant = "groc" }
						};

		_repositoryMock
				.Setup(r => r.GetTransactionsByDateRangeAsync(null, null, It.IsAny<CancellationToken>()))
				.ReturnsAsync(rawTransactions);

		var result = await _service.GetFilteredTransactionsAsync(filters, CancellationToken.None);

		result.Should().HaveCount(2);
		result.Should().OnlyContain(t => t.Merchant == "shop");
		_cacheMock.Verify(c => c.ExecuteWithCache(
				It.IsAny<string>(),
				It.IsAny<Func<Task<IEnumerable<TransactionDto>>>>(),
				It.IsAny<TimeSpan>()), Times.Never);

		_repositoryMock.Verify(r => r.GetTransactionsByDateRangeAsync(
				It.IsAny<DateTime?>(),
				It.IsAny<DateTime?>(),
				It.IsAny<CancellationToken>()), Times.Once);
	}


	[Test]
	public async Task Should_ReturnReturnFilterAndPaginateOnParameters_WhenMultipleParameterIsSet()
	{
		var filters = new TransactionQueryParameters
		{
			Merchant = "shop",
			AccountId = "foo",
			CustomerId = "bar",
			Page = 1,
			PageSize = 2
		};

		var rawTransactions = new List<Transaction>
						{
								new Transaction { AccountId = "foo", CustomerId = "bar", Amount = 100, Merchant = "shop" },
								new Transaction { AccountId = "alpha", CustomerId = "bar", Amount = 200, Merchant = "shop" },
								new Transaction { AccountId = "delta", CustomerId = "omega", Amount = 300, Merchant = "enter" },
								new Transaction { AccountId = "delta", CustomerId = "omega", Amount = 100, Merchant = "groc" }
						};

		_repositoryMock
				.Setup(r => r.GetTransactionsByDateRangeAsync(null, null, It.IsAny<CancellationToken>()))
				.ReturnsAsync(rawTransactions);

		var result = await _service.GetFilteredTransactionsAsync(filters, CancellationToken.None);

		result.Should().HaveCount(1);
		result.Should().OnlyContain(t => t.Merchant == "shop" && t.AccountId == "foo" && t.CustomerId == "bar");
		_cacheMock.Verify(c => c.ExecuteWithCache(
				It.IsAny<string>(),
				It.IsAny<Func<Task<IEnumerable<TransactionDto>>>>(),
				It.IsAny<TimeSpan>()), Times.Never);

		_repositoryMock.Verify(r => r.GetTransactionsByDateRangeAsync(
				It.IsAny<DateTime?>(),
				It.IsAny<DateTime?>(),
				It.IsAny<CancellationToken>()), Times.Once);
	}
}

