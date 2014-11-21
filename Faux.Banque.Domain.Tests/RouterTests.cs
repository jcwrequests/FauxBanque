using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Akka.Routing;
using Faux.Banque.Domain.Actors;
using Akka.Actor;
using Akka.Configuration;
using Faux.Banque.Domain.ValueObjects;
using Faux.Banque.Domain.Contacts;

namespace Faux.Banque.Domain.Tests
{
    [TestClass]
    public class RouterTests
    {
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
                system.ActorOf<CustomerActorTyped>("Worker1");
                system.ActorOf<CustomerActorTyped>("Worker2");
                system.ActorOf<CustomerActorTyped>("Worker3");
                system.ActorOf<CustomerActorTyped>("Worker4");

                var hashGroup = system.ActorOf(Props.Empty.WithRouter(new ConsistentHashingGroup(config)));
                CustomerId id = new CustomerId("1");
                var envelope = new ConsistentHashableEnvelope(new CreateCustomer("John", "Smith", id), id);
                hashGroup.Tell(envelope);

            }
        }
    }
}
