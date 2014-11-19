using Faux.Banque.Domain.Contacts;
using Faux.Banque.Domain.Interfaces;
using Faux.Banque.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Faux.Banque.Domain.Aggregates
{
    public class CustomerState : ICustomerMutations
    {
        
        public CustomerState(IEnumerable<IEvent> events)
        {
            this.Accounts = new Dictionary<AccountId, IAccount>();
            foreach (var e in events)
            {
                Mutate(e);
            }
        }

        internal void Mutate(IEvent e)
        {
            ((dynamic)this).When((dynamic)e);
        }

        public CustomerId CustomerId { private set; get; }
        public string FirstName { private set; get; }
        public string LastName { private set; get; }
        public Dictionary<AccountId, IAccount> Accounts { private set; get; }

        public void When(CustomerCreated e)
        {
            this.CustomerId = e.CustomerId;
            this.FirstName = e.FirstName;
            this.LastName = e.LastName;
        }

        public void When(CheckingAccountOpened e)
        {
            Accounts.Add(e.AccountId, new CheckingAccount(e.CustomerId, e.AccountId, e.OpeningDeposit,e.DateOpened));
        }

        public void When(SavingsAccountOpened e)
        {
            Accounts.Add(e.AccountId,new SavingsAccount(e.CustomerId, e.AccountId, e.OpeningDeposit,e.DateOpened));
        }

        public void When(MoneyTransfered e)
        {
            var source = Accounts[e.SourceAccount];
            var destination = Accounts[e.DestinationAccount];

            source.Post(new Debit(e.DateTransfered, e.AmountTransfered));
            destination.Post(new Credit(e.DateTransfered, e.AmountTransfered));
        }
    }
}
