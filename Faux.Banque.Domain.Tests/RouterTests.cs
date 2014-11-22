using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Akka.Routing;
using Faux.Banque.Domain.Actors;
using Akka.Actor;
using Akka.Configuration;
using Faux.Banque.Domain.ValueObjects;
using Faux.Banque.Domain.Contacts;
using Faux.Banque.Domain.Tests.EventStore;
using System.Threading.Tasks;
using Faux.Banque.Domain.Aggregates;

namespace Faux.Banque.Domain.Tests
{
    [TestClass]
    public class RouterTests
    {
        static InMemoryEventStore store = new InMemoryEventStore();

        [TestMethod]
        public void ConistentHashCreateAnAccount()
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
                var envelope = new ConsistentHashableEnvelope(new CreateCustomer("John", "Smith", id), 2);
                Task.Delay(500).Wait();

                hashGroup.Tell(envelope);

                Storage.EventStream stream = store.LoadEventStream(id);
               
                Assert.IsTrue(stream.Events.Count > 0);
                
            }
        }

        [TestMethod]
        public void ConistentHashTestExample()
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
                system.ActorOf(Props.Create(() => new Worker()), "Worker1");
                system.ActorOf(Props.Create(() => new Worker()), "Worker2");
                system.ActorOf(Props.Create(() => new Worker()), "Worker3");
                system.ActorOf(Props.Create(() => new Worker()), "Worker4");

                var hashGroup = system.ActorOf(Props.Empty.WithRouter(new ConsistentHashingGroup(config)));
                Task.Delay(500).Wait();

                for (var i = 0; i < 5; i++)
                {
                    for (var j = 0; j < 7; j++)
                    {
                        var message = new HashableMessage
                        {
                            Name = Guid.NewGuid().ToString(),
                            Id = j,
                        };

                        hashGroup.Tell(message);
                    }
                }
                
            }
        }
        
    }
}
