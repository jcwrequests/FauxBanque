using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Akka.TestKit.VsTest;
using Faux.Banque.Domain.Aggregates;
using Faux.Banque.Domain.ValueObjects;
using Faux.Banque.Domain.Contacts;
using Akka.Event;
using Faux.Banque.Domain.Tests.EventStore;
using Faux.Banque.Domain.Actors;

namespace Faux.Banque.Domain.Tests
{
   
    [TestClass]
    public class CustomerActorTests : TestKit
    {
        static InMemoryEventStore store = new InMemoryEventStore();

        [TestCategory("Actors")]
        [TestMethod]
        public void Create_Customer()
        {
            
            var actor = ActorOfAsTestActorRef<CustomerActor>(() => new CustomerActor(store), "cusomters");

            CustomerId id = new CustomerId("1");
            actor.Tell(new CreateCustomer("John", "Smith", id));
            Storage.EventStream stream = store.LoadEventStream(id);
            CustomerState state = new CustomerState(stream.Events);
            Assert.IsTrue(state.CustomerId.Equals(id));
        }
    }
}
