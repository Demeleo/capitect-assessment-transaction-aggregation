using Microsoft.Extensions.Caching.Hybrid;
using TransactionAggregator.Application.Interfaces;

namespace TransactionAggregator.Infrastructure.Caching
{
	public class CacheService: ICacheService
	{
		private readonly HybridCache _cache;
		public CacheService(HybridCache cache)
		{
			_cache = cache;
		}
		public async Task<T> ExecuteWithCache<T>(string key, Func<Task<T>> getDataFunction, TimeSpan expiration)
		{
			var options = new HybridCacheEntryOptions
			{
				Expiration = expiration
			};

			return await _cache.GetOrCreateAsync<T>(
					key,
					async _ => await getDataFunction(),
					options);
		}
	}
}
