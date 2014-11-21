using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Akka.Routing;
using Faux.Banque.Domain.Actors;
using Akka.Actor;
using Akka.Configuration;
using Faux.Banque.Domain.ValueObjects;
using Faux.Banque.Domain.Contacts;
using Faux.Banque.Domain.Tests.EventStore;

namespace Faux.Banque.Domain.Tests
{
    [TestClass]
    public class RouterTests
    {
        static InMemoryEventStore store = new InMemoryEventStore();

        [TestMethod]
        public void WireUpConistentHash()
        {
            
            var config = ConfigurationFactory.ParseString(@"
routees.paths = [
    ""akka://MySystem/user/Worker1"" #testing full path
    user/Worker2
    user/Worker3
    user/Worker4
]");

            using(var system = ActorSystem.Create("MySystem"))
            {
                system.ActorOf(Props.Create(() => new CustomerActorTyped(store)), "Worker1");
                system.ActorOf(Props.Create(() => new CustomerActorTyped(store)), "Worker2");
                system.ActorOf(Props.Create(() => new CustomerActorTyped(store)), "Worker3");
                system.ActorOf(Props.Create(() => new CustomerActorTyped(store)), "Worker4");

                var hashGroup = system.ActorOf(Props.Empty.WithRouter(new ConsistentHashingGroup(config)));
                CustomerId id = new CustomerId("1");
                var envelope = new ConsistentHashableEnvelope(new CreateCustomer("John", "Smith", id), id);
                hashGroup.Tell(envelope);

            }
        }
    }
}
