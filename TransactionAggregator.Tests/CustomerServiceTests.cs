using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TransactionAggregator.Application.DTOs;
using TransactionAggregator.Application.Interfaces;
using TransactionAggregator.Application.Requests;
using TransactionAggregator.Application.Services;
using TransactionAggregator.Domain.Entities;

namespace TransactionAggregator.Tests;
[TestFixture]
public class CustomerServiceTests
{
	private Mock<ICustomerRepository> _repositoryMock;
	private Mock<ICacheService> _cacheMock;
	private Mock<ILogger<CustomerService>> _loggerMock;
	private CustomerService _service;

	[SetUp]
	public void SetUp()
	{
		_repositoryMock = new Mock<ICustomerRepository>();
		_cacheMock = new Mock<ICacheService>();
		_loggerMock = new Mock<ILogger<CustomerService>>();
		_service = new CustomerService(_repositoryMock.Object, _cacheMock.Object, _loggerMock.Object);
	}

	[Test]
	public async Task Should_ReturnFilterAndPaginateCachedResult_WhenCacheKeyIsPresent()
	{
		var filters = new CustomerQueryParameters
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

		var result = await _service.GetTransactions("bar", filters, CancellationToken.None);

		result.Should().BeEquivalentTo(cachedResult);
		_cacheMock.Verify(c => c.ExecuteWithCache(
				It.IsAny<string>(),
				It.IsAny<Func<Task<IEnumerable<TransactionDto>>>>(),
				It.IsAny<TimeSpan>()), Times.Once);

		_repositoryMock.Verify(r => r.GetTransactionsByDateRangeAsync("bar",
				It.IsAny<DateTime?>(),
				It.IsAny<DateTime?>(),
				It.IsAny<CancellationToken>()), Times.Never);
	}

	[Test]
	public async Task Should_ReturnFilterAndPaginateNonCachedResult_WhenNoCacheKey()
	{
		var filters = new CustomerQueryParameters
		{
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
				.Setup(r => r.GetTransactionsByDateRangeAsync("bar", null, null, It.IsAny<CancellationToken>()))
				.ReturnsAsync(rawTransactions);

		var result = await _service.GetTransactions("bar", filters, CancellationToken.None);

		result.Should().HaveCount(2);
		result.Should().OnlyContain(t => t.CustomerId == "bar");
		_cacheMock.Verify(c => c.ExecuteWithCache(
				It.IsAny<string>(),
				It.IsAny<Func<Task<IEnumerable<TransactionDto>>>>(),
				It.IsAny<TimeSpan>()), Times.Never);

		_repositoryMock.Verify(r => r.GetTransactionsByDateRangeAsync("bar",
				It.IsAny<DateTime?>(),
				It.IsAny<DateTime?>(),
				It.IsAny<CancellationToken>()), Times.Once);
	}

	[Test]
	public async Task Should_ReturnReturnFilterAndPaginateOnAccountId_WhenAccountIdParameterIsSet()
	{
		var filters = new CustomerQueryParameters
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
				.Setup(r => r.GetTransactionsByDateRangeAsync("bar", null, null, It.IsAny<CancellationToken>()))
				.ReturnsAsync(rawTransactions);

		var result = await _service.GetTransactions("bar", filters, CancellationToken.None);

		result.Should().HaveCount(1);
		result.Should().OnlyContain(t => t.AccountId == "alpha");
		_cacheMock.Verify(c => c.ExecuteWithCache(
				It.IsAny<string>(),
				It.IsAny<Func<Task<IEnumerable<TransactionDto>>>>(),
				It.IsAny<TimeSpan>()), Times.Never);

		_repositoryMock.Verify(r => r.GetTransactionsByDateRangeAsync("bar",
				It.IsAny<DateTime?>(),
				It.IsAny<DateTime?>(),
				It.IsAny<CancellationToken>()), Times.Once);
	}
}

