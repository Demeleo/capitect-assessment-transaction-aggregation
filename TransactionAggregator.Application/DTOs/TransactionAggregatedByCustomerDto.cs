using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionAggregator.Application.DTOs
{
	public class TransactionAggregatedByCustomerDto
	{
		public string CustomerId { get; set; }
		public decimal TotalAmount { get; set; }
		public int TransactionCount { get; set; }
		public Dictionary<string, decimal> CategoryTotalAmounts { get; set; }

	}
}
