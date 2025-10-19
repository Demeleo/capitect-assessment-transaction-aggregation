using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionAggregator.Infrastructure.Factories
{
	public static class PolicyFactory
	{
		public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger logger, int retryCount = 3)
		{
			var delays = Enumerable.Range(0, retryCount).Select(i => TimeSpan.FromSeconds(Math.Pow(2, i))).ToArray();

			return HttpPolicyExtensions
				.HandleTransientHttpError()
				.OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
				.WaitAndRetryAsync(delays, onRetry: (outcome, timespan, retryAttempt, context) =>
				{
					logger?.LogWarning("HTTP retry {RetryAttempt} after {Delay}ms. Reason: {Reason}",
						retryAttempt, timespan.TotalMilliseconds,
						outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
				});
		}

		public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan timeout)
		{
			return Policy.TimeoutAsync<HttpResponseMessage>(timeout, TimeoutStrategy.Optimistic);
		}
	}
}
