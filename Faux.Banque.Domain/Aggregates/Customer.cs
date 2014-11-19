using Faux.Banque.Domain.Contacts;
using Faux.Banque.Domain.Interfaces;
using Faux.Banque.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Faux.Banque.Domain.Aggregates
{
    public class Customer
    {
        public readonly IList<IEvent> Changes = new List<IEvent>();
        readonly CustomerState _state;

        public Customer(IEnumerable<IEvent> events)
        {
            this._state = new CustomerState(events);
        }
        void Apply(IEvent e)
        {
            _state.Mutate(e);
            Changes.Add(e);
        }
        public void CreateCustomer(CustomerId customerId, string firstName, string lastName)
        {
            if (_state.CustomerId != null) throw new Exception("Customer Already Created!");
            this.Apply(new CustomerCreated(firstName,lastName,customerId));
        }
        public void OpenCheckingAccount(CustomerId customerId, AccountId accountId, decimal openingBalance, DateTime openingDate)
        {
            if (_state.CustomerId == null) throw new Exception("Customer Not Created");
            if (_state.CustomerId != customerId) throw new Exception("Wrong Customer");
            if (_state.Accounts.ContainsKey(accountId)) throw new Exception("Account already open");
            this.Apply(new CheckingAccountOpened(customerId,accountId,openingBalance,openingDate));
        }
        public void OpenSavingsAccount(CustomerId customerId, AccountId accountId, decimal openingBalance, DateTime openingDate)
        {
            if (_state.CustomerId == null) throw new Exception("Customer Not Created");
            if (_state.CustomerId != customerId) throw new Exception("Wrong Customer");
            if (_state.Accounts.ContainsKey(accountId)) throw new Exception("Account already open");
            this.Apply(new SavingsAccountOpened(customerId, accountId, openingBalance, openingDate));
        }
        public void TransferMoney(CustomerId customerId,AccountId sourceAccount, AccountId destinationAccount, decimal amount)
        {
            if (_state.CustomerId == null) throw new Exception("Customer Not Created");
            if (_state.CustomerId != customerId) throw new Exception("Wrong Customer");
            if (!_state.Accounts.ContainsKey(sourceAccount)) throw new Exception("Source Account Does not Exist");
            if (!_state.Accounts.ContainsKey(destinationAccount)) throw new Exception("Source Account Does not Exist");
            this.Apply(new MoneyTransfered(customerId, sourceAccount, destinationAccount, amount, DateTime.Now));
        }
    }
}
