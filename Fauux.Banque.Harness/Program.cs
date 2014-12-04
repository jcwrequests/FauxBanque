using Akka.Actor;
using Akka.Configuration;
using Akka.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using Castle.MicroKernel.Registration;

namespace Fauux.Banque.Harness
{
    class Program
    {
        static void Main(string[] args)
        {
            //HardCoded();
            Container();
        }
        static void HardCoded()
        {
            var config = ConfigurationFactory.ParseString(@"
routees.paths = [
    ""akka://MySystem/user/Worker1"" #testing full path
    user/Worker2
    user/Worker3
    user/Worker4
]");

            using (var system = ActorSystem.Create("MySystem"))
            {


                system.ActorOf(Props.Create(() => new TypedWorker()), "Worker1");
                system.ActorOf(Props.Create(() => new TypedWorker()), "Worker2");
                system.ActorOf(Props.Create(() => new TypedWorker()), "Worker3");
                system.ActorOf(Props.Create(() => new TypedWorker()), "Worker4");

                var hashGroup = system.ActorOf(Props.Empty.WithRouter(new ConsistentHashingGroup(config)));

                Task.Delay(500).Wait();

                for (var i = 0; i < 5; i++)
                {
                    for (var j = 0; j < 7; j++)
                    {

                        TypedActorMessage msg = new TypedActorMessage { Id = j, Name = Guid.NewGuid().ToString() };
                        AnotherMessage ms = new AnotherMessage { Id = j, Name = msg.Name };

                        var envelope = new ConsistentHashableEnvelope(ms, msg.Id);

                        hashGroup.Tell(msg);
                        hashGroup.Tell(envelope);

                    }
                }

            }
            Console.ReadLine();
        }
        static void Container()
        {
            var config = ConfigurationFactory.ParseString(@"
            routees.paths = [
                ""akka://MySystem/user/Worker1"" #testing full path
                ]");

            IWindsorContainer container = new WindsorContainer();
            container.Register(Component.For<TypedWorker>().Named("TypedWorker").LifestyleTransient());
            //container.Register(Component.For<ActorSystem>());

            ApplicationConfig appConfig = new ApplicationConfig(container,config);
            var system = appConfig.actorSystem("Test");

            ///system.ActorOf(Props.Create(() => container.Resolve<TypedWorker>()),)
            ///
            system.ActorOf(DIExtension.DIExtensionProvider.Get(system).Props("TypedWorker"), "Worker1");
     
            

            var hashGroup = system.ActorOf(Props.Empty.WithRouter(new ConsistentHashingGroup(config)));

            Task.Delay(500).Wait();

            for (var i = 0; i < 5; i++)
            {
                for (var j = 0; j < 7; j++)
                {

                    TypedActorMessage msg = new TypedActorMessage { Id = j, Name = Guid.NewGuid().ToString() };
                    AnotherMessage ms = new AnotherMessage { Id = j, Name = msg.Name };

                    var envelope = new ConsistentHashableEnvelope(ms, msg.Id);

                    hashGroup.Tell(msg);
                    hashGroup.Tell(envelope);

                }
            }
            Console.ReadLine();
        }
    }
}
