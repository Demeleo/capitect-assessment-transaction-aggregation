using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionAggregator.Application.DTOs;
using TransactionAggregator.Application.Requests;

namespace TransactionAggregator.Application.Interfaces
{
	public interface ITransactionService
	{
		Task<IEnumerable<TransactionDto>> GetFilteredTransactionsAsync(TransactionQueryParameters filters, CancellationToken cancellationToken);
	}
}
