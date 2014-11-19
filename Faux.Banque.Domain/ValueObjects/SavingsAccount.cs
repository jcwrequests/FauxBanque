using Faux.Banque.Domain.Contacts;
using Faux.Banque.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Faux.Banque.Domain.ValueObjects
{
    public class SavingsAccount : IAccount
    {
        readonly CustomerId customerId;
        readonly AccountId accountId;
        readonly decimal totalBalance;
        List<LedgerTransaction> transactions;

        public SavingsAccount(CustomerId customerId,
                                AccountId accountId,
                                decimal totalBalance,
                                DateTime dateOpened)
        {
            if (customerId == null) throw new ArgumentNullException("customerId");
            if (accountId == null) throw new ArgumentNullException("accountId");

            this.customerId = customerId;
            this.accountId = accountId;
            this.totalBalance = totalBalance;
            transactions = new List<LedgerTransaction>();
            this.transactions.Add(new Credit(dateOpened, totalBalance));
        }
        public CustomerId CustomerId
        {
            get { return this.customerId; }
        }

        public AccountId AccountId
        {
            get { return this.accountId; }
        }

        public decimal TotalBalance
        {
            get { return transactions.Sum(t => t.Amount); }
        }


        public void Post(LedgerTransaction transaction)
        {
            transactions.Add(transaction);
        }
    }
}
