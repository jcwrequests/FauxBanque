using Faux.Banque.Domain.Contacts;
using Faux.Banque.Domain.Exceptions;
using Faux.Banque.Domain.Interfaces;
using Faux.Banque.Domain.Storage;
using Faux.Banque.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Faux.Banque.Domain.Aggregates
{
    public class CustomerApplicationService : IApplicationService
    {
        readonly IEventStore eventStore;

        public CustomerApplicationService(IEventStore eventStore)
        {
            if (eventStore == null) throw new ArgumentNullException("eventStore");
            this.eventStore = eventStore;

        }
        void Update(CustomerId customerId, Action<Customer> execute)
        {
            while (true)
            {
                EventStream eventStream = eventStore.LoadEventStream(customerId);
                Customer customer = new Customer(eventStream.Events);

                execute(customer);

                try
                {
                    eventStore.AppendToStream(customerId, eventStream.Version, customer.Changes);
                    return;
                }
                catch (OptimisticConcurrencyException ex)
                {
                    foreach (var customerEvent in customer.Changes)
                    {
                        foreach (var actualEvent in ex.ActualEvents)
                        {
                            if (ConflictsWith(customerEvent, actualEvent))
                            {
                                var msg = string.Format("Conflict between {0} and {1}", customerEvent, actualEvent);
                                throw new RealConcurrencyException(msg, ex);
                            }
                        }
                    }
                    eventStore.AppendToStream(customerId, ex.ActualVersion, customer.Changes);
                }
            }
        }
        static bool ConflictsWith(IEvent x, IEvent y)
        {
            return x.GetType() == y.GetType();
        }
        public void Execute(ICommand cmd)
        {
            ((dynamic)this).When((dynamic)cmd);
        }

        public void When(CreateCustomer cmd)
        {
            Update(cmd.CustomerId, 
                   a => a.CreateCustomer(cmd.CustomerId, 
                                         cmd.FirstName, 
                                         cmd.LastName));
        }
        public void When(OpenSavingsAccount cmd)
        {
            Update(cmd.CustomerId, 
                   a => a.OpenCheckingAccount(cmd.CustomerId, 
                                              cmd.AccountId, 
                                              cmd.OpeningDeposit,
                                              DateTime.Now));
        }
        public void When(OpenCheckingAccount cmd)
        {
            Update(cmd.CustomerId,
                    a => a.OpenCheckingAccount(cmd.CustomerId,
                                               cmd.AccountId,
                                               cmd.OpeningDeposit,
                                               DateTime.Now));
        }
        public void When(TransferMoney cmd)
        {
            Update(cmd.CustomerId,
                   a => a.TransferMoney(cmd.CustomerId,
                                        cmd.SourceAccount,
                                        cmd.DestinationAccount,
                                        cmd.AmountToTransfer));
        }
    }
}
