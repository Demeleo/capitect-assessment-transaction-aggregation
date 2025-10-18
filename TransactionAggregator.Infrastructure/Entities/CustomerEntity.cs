using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionAggregator.Infrastructure.Entities
{
	public class CustomerEntity
	{
		public string Id { get; set; } = string.Empty;
		public ICollection<AccountEntity> Accounts { get; set; } = [];
	}
}
