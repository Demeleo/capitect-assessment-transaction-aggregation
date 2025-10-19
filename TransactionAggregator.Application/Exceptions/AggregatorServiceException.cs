using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionAggregator.Application.Exceptions
{
	public class AggregatorServiceException: AggregatorException
	{
		public AggregatorServiceException(string message, Exception? inner = null) : base(message, inner) { }
	}
}
