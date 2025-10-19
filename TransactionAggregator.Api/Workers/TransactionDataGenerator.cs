using TransactionAggregator.Application.Interfaces;

namespace TransactionAggregator.Api.Workers
{
	public class TransactionDataGenerator : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<TransactionDataGenerator> _logger;
		public TransactionDataGenerator(IServiceScopeFactory scopeFactory, ILogger<TransactionDataGenerator> logger)
		{
			_scopeFactory = scopeFactory;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Transaction data generator started.");
			using var scope = _scopeFactory.CreateScope();

			var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionsRetrievalService>();

			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					_logger.LogInformation("Generating new set of transactions...");
					await transactionService.RetrieveAllTransactionDataFromVendors(cancellationToken);
					_logger.LogInformation("Transactions generated successfully.");
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error during transactions data generation.");
				}

				await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
			}
		}
	}
}
