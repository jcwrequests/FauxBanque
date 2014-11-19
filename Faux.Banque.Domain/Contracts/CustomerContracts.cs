using Faux.Banque.Domain.Interfaces;
using Faux.Banque.Domain.ValueObjects;
using System;
namespace Faux.Banque.Domain.Contacts
{
    public class CreateCustomer : ICommand
    {
        public CreateCustomer(string firstName,
                              string lastName,
                              CustomerId customerId)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.CustomerId = customerId;
        }
        public string FirstName { private set; get; }
        public string LastName { private set; get; }
        public CustomerId CustomerId {private set;get;}
    }

    public class CustomerCreated : IEvent
    {
        public CustomerCreated(string firstName,
                              string lastName,
                              CustomerId customerId)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.CustomerId = customerId;
        }
        public string FirstName { private set; get; }
        public string LastName { private set; get; }
        public CustomerId CustomerId {private set;get;}
    }

    public class OpenCheckingAccount : ICommand
    {
        public OpenCheckingAccount(CustomerId customerId,
                                   AccountId accountId,
                                   decimal openingDeposit)
        {
            this.CustomerId = customerId;
            this.AccountId = accountId;
            this.OpeningDeposit = openingDeposit;
        }

        public CustomerId CustomerId { private set; get; }
        public AccountId AccountId {private set; get;}
        public decimal OpeningDeposit {private set; get;}
    }
    public class CheckingAccountOpened : IEvent
    {
        public CheckingAccountOpened(CustomerId customerId,
                                   AccountId accountId,
                                   decimal openingDeposit,
                                   DateTime dateOpened)
        {
            this.CustomerId = customerId;
            this.AccountId = accountId;
            this.OpeningDeposit = openingDeposit;
            this.DateOpened = dateOpened;
        }

        public CustomerId CustomerId { private set; get; }
        public AccountId AccountId {private set; get;}
        public decimal OpeningDeposit {private set; get;}
        public DateTime DateOpened { private set; get; }
    }

    public class OpenSavingsAccount : ICommand
    {
        public OpenSavingsAccount(CustomerId customerId,
                                   AccountId accountId,
                                   decimal openingDeposit)
        {
            this.CustomerId = customerId;
            this.AccountId = accountId;
            this.OpeningDeposit = openingDeposit;
        }

        public CustomerId CustomerId { private set; get; }
        public AccountId AccountId { private set; get; }
        public decimal OpeningDeposit { private set; get; }
    }
    public class SavingsAccountOpened : IEvent
    {
        public SavingsAccountOpened(CustomerId customerId,
                                   AccountId accountId,
                                   decimal openingDeposit,
                                   DateTime dateOpened)
        {
            this.CustomerId = customerId;
            this.AccountId = accountId;
            this.OpeningDeposit = openingDeposit;
            this.DateOpened = dateOpened;
        }

        public CustomerId CustomerId { private set; get; }
        public AccountId AccountId { private set; get; }
        public decimal OpeningDeposit { private set; get; }
        public DateTime DateOpened { private set; get; }
    }

    public class TransferMoney : ICommand
    {
        public TransferMoney (CustomerId customerId,
                               AccountId sourceAccount,
                               AccountId destinationAccount,
                               decimal amountToTransfer)
	    {
            this.CustomerId = customerId;
            this.SourceAccount = sourceAccount;
            this.DestinationAccount = destinationAccount;
            this.AmountToTransfer = amountToTransfer;
	    }
        public CustomerId CustomerId { private set; get; }
        public AccountId SourceAccount { private set; get; }
        public AccountId DestinationAccount { private set; get; }
        public decimal AmountToTransfer {private set; get;}

    }

    public class MoneyTransfered : IEvent
    {
        public MoneyTransfered (CustomerId customerId,
                               AccountId sourceAccount,
                               AccountId destinationAccount,
                               decimal amountTransfered,
                               DateTime dateTransfered)
	    {
            this.CustomerId = customerId;
            this.SourceAccount = sourceAccount;
            this.DestinationAccount = destinationAccount;
            this.AmountTransfered = amountTransfered;
            this.DateTransfered = dateTransfered;
	    }
        public CustomerId CustomerId { private set; get; }
        public AccountId SourceAccount { private set; get; }
        public AccountId DestinationAccount { private set; get; }
        public decimal AmountTransfered {private set; get;}
        public DateTime DateTransfered { private set; get; }

    }

    public interface ICustomerMutations
    {
        void When(CustomerCreated e);
        void When(CheckingAccountOpened e);
        void When(SavingsAccountOpened e);
        void When(MoneyTransfered e);
    }

}