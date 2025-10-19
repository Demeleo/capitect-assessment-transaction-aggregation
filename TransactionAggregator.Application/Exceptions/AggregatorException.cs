using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionAggregator.Application.Exceptions
{
	public class AggregatorException: Exception
	{
		protected AggregatorException(string message) : base(message) { }
		protected AggregatorException(string message, Exception inner) : base(message, inner) { }
	}
}
