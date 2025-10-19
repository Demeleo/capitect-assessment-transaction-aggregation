using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionAggregator.Application.Telemetry
{
	public static class ApplicationTelemetry
	{
		public static readonly ActivitySource ActivitySource = new("TransactionAggregator.Application");
	}
}
