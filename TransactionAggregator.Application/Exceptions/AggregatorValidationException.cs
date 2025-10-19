using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionAggregator.Application.Exceptions
{
	public class AggregatorValidationException: AggregatorException
	{
		public IDictionary<string, string[]> Errors { get; }

		public AggregatorValidationException(string message, IDictionary<string, string[]>? errors = null) : base(message)
		{
			Errors = errors ?? new Dictionary<string, string[]>();
		}
	}
}
