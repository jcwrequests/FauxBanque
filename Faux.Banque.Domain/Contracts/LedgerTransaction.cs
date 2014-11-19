using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Faux.Banque.Domain.Contracts
{
    public abstract class LedgerTransaction
    {
        public LedgerTransaction(DateTime transactionDate, decimal amount)
        {
            this.Amount = amount;
            this.TransactionDate = transactionDate;
        }
        public readonly DateTime TransactionDate;
        public readonly decimal Amount;
    }
}
