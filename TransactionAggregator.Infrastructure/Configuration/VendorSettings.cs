using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionAggregator.Infrastructure.Configuration
{
	public class VendorSettings
	{
		public int DefaultTimeoutSeconds { get; set; }
		public int DefaultRetryCount { get; set; }
		public VendorConfiguration VendorA { get; set; }
		public VendorConfiguration VendorB { get; set; }
	}

	public class VendorConfiguration
	{
		public string BaseUrl { get; set; }
	}
}
