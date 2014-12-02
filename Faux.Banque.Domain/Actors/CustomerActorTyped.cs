using Akka.Actor;
using Faux.Banque.Domain.Aggregates;
using Faux.Banque.Domain.Contacts;
using Faux.Banque.Domain.Storage;
using Faux.Banque.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faux.Banque.Domain.Actors
{
    public class CustomerActorTyped : TypedActor, 
        IHandle<CreateCustomer>, 
        IHandle<OpenSavingsAccount>,
        IHandle<OpenCheckingAccount>,
        IHandle<TransferMoney>
    {

        readonly IEventStore eventStore;

        public CustomerActorTyped(IEventStore eventStore)
        {
             if (eventStore == null) throw new ArgumentNullException("eventStore");
            this.eventStore = eventStore;
        }

        public void Handle(CreateCustomer message)
        {
            Customer customer = GetCustomer(message.CustomerId);
            customer.CreateCustomer(message.CustomerId, message.FirstName, message.LastName);
            AppendToStream(message.CustomerId, customer);
            
        }

        public void Handle(OpenSavingsAccount message)
        {
            Customer customer = GetCustomer(message.CustomerId);
            customer.OpenCheckingAccount(message.CustomerId, message.AccountId, message.OpeningDeposit, DateTime.Now);
            AppendToStream(message.CustomerId, customer);
        }

        public void Handle(TransferMoney message)
        {
            Customer customer = GetCustomer(message.CustomerId);
            customer.TransferMoney(message.CustomerId, message.SourceAccount, message.DestinationAccount, message.AmountToTransfer);
            AppendToStream(message.CustomerId, customer);
        }

        public void Handle(OpenCheckingAccount message)
        {
            Customer customer = GetCustomer(message.CustomerId);
            customer.OpenCheckingAccount(message.CustomerId, message.AccountId,message.OpeningDeposit,DateTime.Now);
            AppendToStream(message.CustomerId, customer);
        }

        private Customer GetCustomer(CustomerId customerId)
        {
            EventStream eventStream = eventStore.LoadEventStream(customerId);
            Customer customer = new Customer(eventStream.Events);
            return customer;
        }
        private void AppendToStream(CustomerId customerId,Customer customer)
        {
            EventStream eventStream = eventStore.LoadEventStream(customerId);
            eventStore.AppendToStream(customerId, eventStream.Version, customer.Changes);
        }

    }
}
