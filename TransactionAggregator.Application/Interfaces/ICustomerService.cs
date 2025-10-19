using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAggregator.Application.DTOs;
using TransactionAggregator.Application.Requests;

namespace TransactionAggregator.Application.Interfaces
{
	public interface ICustomerService
	{
		Task<IEnumerable<TransactionDto>> GetTransactions(string customerId, CustomerQueryParameters filters, CancellationToken cancellationToken);
	}
}
