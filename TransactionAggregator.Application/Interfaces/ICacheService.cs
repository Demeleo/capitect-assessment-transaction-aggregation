using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionAggregator.Application.Interfaces
{
	public interface ICacheService
	{
		Task<T> ExecuteWithCache<T>(string key, Func<Task<T>> getDataFunction, TimeSpan expiration);
	}
}
