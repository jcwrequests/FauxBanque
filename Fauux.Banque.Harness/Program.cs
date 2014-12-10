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
using Autofac;
namespace Fauux.Banque.Harness
{
    class Program
    {
        static void Main(string[] args)
        {
            HardCoded();
            ContainerWindsor();
            ContainerAutoFac();
            ContainerNinject();
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
        static void ContainerWindsor()
        {
            var config = ConfigurationFactory.ParseString(@"
routees.paths = [
    ""akka://MySystem/user/Worker1"" #testing full path
    user/Worker2
    user/Worker3
    user/Worker4
]");

            IWindsorContainer container = new WindsorContainer();
            container.Register(Component.For<TypedWorker>().Named("TypedWorker").LifestyleTransient());

            WindsorConfiguration appConfig = new WindsorConfiguration(container);
            var system = appConfig.CreateActorSystem("Test");

            system.ActorOf<TypedWorker>("Worker1");
            system.ActorOf<TypedWorker>("Worker2");
            system.ActorOf<TypedWorker>("Worker3");
            system.ActorOf<TypedWorker>("Worker4");
            
            var hashGroup = system.ActorOf(Props.Empty.WithRouter(new ConsistentHashingGroup(config)));

            Task.Delay(2000).Wait();

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
        static void ContainerNinject()
        {
            var config = ConfigurationFactory.ParseString(@"
routees.paths = [
    ""akka://MySystem/user/Worker1"" #testing full path
    user/Worker2
    user/Worker3
    user/Worker4
]");

            Ninject.IKernel container = new Ninject.StandardKernel();
            container.Bind<TypedWorker>().To(typeof(TypedWorker));

            NinjectConfiuration appConfig = new NinjectConfiuration(container);
            var system = appConfig.CreateActorSystem("Test");

            system.ActorOf<TypedWorker>("Worker1");
            system.ActorOf<TypedWorker>("Worker2");
            system.ActorOf<TypedWorker>("Worker3");
            system.ActorOf<TypedWorker>("Worker4");

            var hashGroup = system.ActorOf(Props.Empty.WithRouter(new ConsistentHashingGroup(config)));

            Task.Delay(2000).Wait();

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
        static void ContainerAutoFac()
        {
            var config = ConfigurationFactory.ParseString(@"
routees.paths = [
    ""akka://MySystem/user/Worker1"" #testing full path
    user/Worker2
    user/Worker3
    user/Worker4
]");
            var builder = new Autofac.ContainerBuilder();
            builder.RegisterType<TypedWorker>();

            Autofac.IContainer container = builder.Build();


            
            AutoFacConfiguration appConfig = new AutoFacConfiguration(container);
            var system = appConfig.CreateActorSystem("Test");

            system.ActorOf<TypedWorker>("Worker1");
            system.ActorOf<TypedWorker>("Worker2");
            system.ActorOf<TypedWorker>("Worker3");
            system.ActorOf<TypedWorker>("Worker4");
            
            var hashGroup = system.ActorOf(Props.Empty.WithRouter(new ConsistentHashingGroup(config)));

            Task.Delay(2000).Wait();

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
