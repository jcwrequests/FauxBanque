using Faux.Banque.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Faux.Banque.Domain.ValueObjects
{
    public class Debit : LedgerTransaction
    {
        public Debit(DateTime transactionDate, decimal amount) : base(transactionDate, amount *-1) { }
    }
    public class Credit: LedgerTransaction
    {
        public Credit(DateTime transactionDate, decimal amount) : base(transactionDate, amount) { }
    }
}
