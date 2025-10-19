using Bogus.DataSets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Buffers.Text;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using TransactionAggregator.Application.DTOs;
using TransactionAggregator.Application.Exceptions;
using TransactionAggregator.Application.Telemetry;
using TransactionAggregator.Domain.Entities;
using TransactionAggregator.Domain.Interfaces;
using TransactionAggregator.Infrastructure.Configuration;
using TransactionAggregator.Infrastructure.Vendors.DataGenerator;

namespace TransactionAggregator.Infrastructure.Vendors
{
	public class VendorB : IFinancialVendor
	{
		private readonly HttpClient _client;
		private readonly ILogger<VendorB> _logger;
		private readonly VendorSettings _settings;

		public VendorB(IHttpClientFactory httpClientFactory, ILogger<VendorB> logger, IOptions<VendorSettings> options)
		{
			_client = httpClientFactory.CreateClient("VendorB");
			_logger = logger;
			_settings = options?.Value ?? new VendorSettings();
		}

		public async Task<IEnumerable<Transaction>> GetTransactionsAsync(CancellationToken cancellationToken = default)
		{
			using var activity = ApplicationTelemetry.ActivitySource.StartActivity("VendorB.GetTransactions", ActivityKind.Server);
			try
			{
				activity?.SetTag("vendor.name", "Vendor B");

				var baseUrl = _settings?.VendorB?.BaseUrl;
				var endpoint = "/transactions";

				_logger.LogInformation("VendorB: calling {Base}{Endpoint}", baseUrl, endpoint);
				using var response = await _client.GetAsync("/transactions", cancellationToken);
				response.EnsureSuccessStatusCode();
				var transactions = await response.Content.ReadFromJsonAsync<IEnumerable<Transaction>>(cancellationToken: cancellationToken)
					?? [];
				return transactions;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "VendorB unexpected error");
				return [];
			}
		}
	}
}
