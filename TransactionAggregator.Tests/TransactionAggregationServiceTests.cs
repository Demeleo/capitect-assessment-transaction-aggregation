using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TransactionAggregator.Application.DTOs;
using TransactionAggregator.Application.Interfaces;
using TransactionAggregator.Application.Services;

namespace TransactionAggregator.Tests;

[TestFixture]
public class TransactionAggregationServiceTests
{
	private Mock<ITransactionRepository> _repositoryMock;
	private Mock<ILogger<TransactionAggregationService>> _loggerMock;
	private TransactionAggregationService _service;

	[SetUp]
	public void SetUp()
	{
		_repositoryMock = new Mock<ITransactionRepository>();
		_loggerMock = new Mock<ILogger<TransactionAggregationService>>();
		_service = new TransactionAggregationService(_repositoryMock.Object, _loggerMock.Object);
	}

	[Test]
	public async Task GetAggregatedByCategoryAsync_Should_MapRepositoryResults_ToDto()
	{
		var repoResult = new List<TransactionAggregatedByCategory>
						{
								new TransactionAggregatedByCategory
								{
										Category = "foo",
										TotalAmount = 150,
										TransactionCount = 3,
										AmountPerCustomer = new Dictionary<string, decimal>
										{
												{ "foo", 100 },
												{ "bar", 50 }
										}
								}
						};

		_repositoryMock
				.Setup(r => r.GetAggregatedByCategoryAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(repoResult);

		var result = await _service.GetAggregatedByCategoryAsync(CancellationToken.None);

		result.Should().HaveCount(1);
		result.Should().ContainSingle(r =>
				r.Category == "foo" &&
				r.TotalAmount == 150 &&
				r.TransactionCount == 3 &&
				r.AmountPerCustomer["foo"] == 100 &&
				r.AmountPerCustomer["bar"] == 50);
	}

	[Test]
	public async Task GetAggregatedByCustomerAsync_Should_MapRepositoryResults_ToDto()
	{
		var repoResult = new List<TransactionAggregatedByCustomer>
						{
								new TransactionAggregatedByCustomer
								{
										CustomerId = "foo",
										TotalAmount = 200,
										TransactionCount = 4,
										CategoryTotalAmounts = new Dictionary<string, decimal>
										{
												{ "foo", 120 },
												{ "bar", 80 }
										}
								}
						};

		_repositoryMock
				.Setup(r => r.GetAggregatedByCustomerAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(repoResult);

		var result = await _service.GetAggregatedByCustomerAsync(CancellationToken.None);

		result.Should().HaveCount(1);
		result.Should().ContainSingle(r =>
				r.CustomerId == "foo" &&
				r.TotalAmount == 200 &&
				r.TransactionCount == 4 &&
				r.CategoryTotalAmounts["foo"] == 120 &&
				r.CategoryTotalAmounts["bar"] == 80);
	}
}


