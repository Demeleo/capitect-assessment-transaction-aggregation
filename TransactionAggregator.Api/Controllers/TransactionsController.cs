using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransactionAggregator.Application.DTOs;
using TransactionAggregator.Application.Interfaces;
using TransactionAggregator.Application.Requests;
using TransactionAggregator.Application.Services;

namespace TransactionAggregator.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TransactionsController : ControllerBase
	{
		private readonly ITransactionAggregationService _aggregationService;
		private readonly ITransactionService _transactionService;

		public TransactionsController(ITransactionAggregationService aggregationService, ITransactionService transactionService)
		{
			_aggregationService = aggregationService;
			_transactionService = transactionService;
		}

		/// <summary>
		/// Retrieves a filtered list of transactions based on the specified query parameters.
		/// </summary>
		/// <remarks>The method returns an HTTP 200 response with the filtered transactions if successful. If no
		/// transactions match the filters, an empty collection is returned.</remarks>
		/// <param name="filters">The query parameters used to filter the transactions. This includes criteria such as date range, transaction type,
		/// or other relevant filters.</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests, allowing the operation to be canceled if needed.</param>
		/// <returns>An <see cref="ActionResult{T}"/> containing an <see cref="IEnumerable{T}"/> of <see cref="TransactionDto"/>
		/// objects that match the specified filters.</returns>
		[HttpGet]
		public async Task<ActionResult<IEnumerable<TransactionDto>>> GetFilteredTransactions(
				[FromQuery] TransactionQueryParameters filters,
				CancellationToken cancellationToken)
		{
			var result = await _transactionService.GetFilteredTransactionsAsync(filters, cancellationToken);
			return Ok(result);
		}

		/// <summary>
		/// Retrieves a collection of transactions aggregated by customer.
		/// </summary>
		/// <remarks>This method returns an HTTP 200 response with the aggregated transaction data if
		/// successful.</remarks>
		/// <param name="cancellationToken">A token to monitor for cancellation requests. Passing a cancellation token allows the operation to be canceled.</param>
		/// <returns>An <see cref="ActionResult{T}"/> containing an <see cref="IEnumerable{T}"/> of <see
		/// cref="TransactionAggregatedByCustomerDto"/> objects. The result represents transactions grouped by customer.</returns>
		[HttpGet("aggregates/by-customer")]
		public async Task<ActionResult<IEnumerable<TransactionAggregatedByCustomerDto>>> GetByCustomer(CancellationToken cancellationToken)
		{
			var result = await _aggregationService.GetAggregatedByCustomerAsync(cancellationToken);
			return Ok(result);
		}


		/// <summary>
		/// Retrieves a collection of transactions aggregated by category.
		/// </summary>
		/// <remarks>This method returns a list of aggregated transaction data, grouped by category. The data is
		/// retrieved asynchronously and can be used to analyze transaction trends or summaries based on categories.</remarks>
		/// <param name="cancellationToken">A token to monitor for cancellation requests. Passing a cancellation token allows the operation to be canceled if
		/// needed.</param>
		/// <returns>An <see cref="ActionResult{T}"/> containing an <see cref="IEnumerable{T}"/> of <see
		/// cref="TransactionAggregatedByCategoryDto"/> objects, representing the aggregated transaction data.</returns>
		[HttpGet("aggregates/by-category")]
		public async Task<ActionResult<IEnumerable<TransactionAggregatedByCategoryDto>>> GetByCategory(CancellationToken cancellationToken)
		{
			var result = await _aggregationService.GetAggregatedByCategoryAsync(cancellationToken);
			return Ok(result);
		}

	}
}
