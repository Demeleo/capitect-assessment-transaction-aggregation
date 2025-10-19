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
	public class CustomersController : ControllerBase
	{
		private readonly ITransactionAggregationService _aggregationService;
		private readonly ICustomerService _customerService;

		public CustomersController(ITransactionAggregationService aggregationService, ICustomerService customerService)
		{
			_aggregationService = aggregationService;
			_customerService = customerService;
		}

		/// <summary>
		/// Retrieves a customer's filtered list of transactions based on the specified query parameters.
		/// </summary>
		/// <remarks>The method returns an HTTP 200 response with the filtered transactions if successful. If no
		/// transactions match the filters, an empty collection is returned.</remarks>
		/// <param name="filters">The query parameters used to filter the transactions. This includes criteria such as date range, transaction type,
		/// or other relevant filters.</param>
		/// <param name="cancellationToken">A token to monitor for cancellation requests, allowing the operation to be canceled if needed.</param>
		/// <returns>An <see cref="ActionResult{T}"/> containing an <see cref="IEnumerable{T}"/> of <see cref="TransactionDto"/>
		/// objects that match the specified filters.</returns>
		[HttpGet("{id}/transactions")]
		public async Task<ActionResult<IEnumerable<TransactionDto>>> GetFilteredTransactions(
				string id,
				[FromQuery] CustomerQueryParameters filters,
				CancellationToken cancellationToken)
		{
			var result = await _customerService.GetTransactions(id, filters, cancellationToken);
			return Ok(result);
		}
	}
}
