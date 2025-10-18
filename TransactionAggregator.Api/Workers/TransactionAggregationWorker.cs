using TransactionAggregator.Application.Interfaces;

namespace TransactionAggregator.Api.Workers
{
	public class TransactionAggregationWorker : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<TransactionAggregationWorker> _logger;
		public TransactionAggregationWorker(IServiceScopeFactory scopeFactory, ILogger<TransactionAggregationWorker> logger)
		{
			_scopeFactory = scopeFactory;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Transaction aggregation worker started.");
			using var scope = _scopeFactory.CreateScope();

			var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionsRetrievalService>();

			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					_logger.LogInformation("Running aggregation job...");
					await transactionService.RetrieveAllTransactionDataFromVendors(cancellationToken);
					_logger.LogInformation("Aggregation job completed successfully.");
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error during aggregation job.");
				}

				await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
			}
		}
	}
}
