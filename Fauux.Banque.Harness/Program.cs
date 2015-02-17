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
            //HardCoded();
            ContainerWindsor();
            //ContainerAutoFac();
            //ContainerNinject();
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
            using (var system = ActorSystem.Create("MySystem",config))
            {
                IWindsorContainer container = new WindsorContainer();
                container.Register(Component.For<TypedWorker>().Named("TypedWorker").LifestyleTransient());

                
                WindsorDependencyResolver propsResolver = 
                    new WindsorDependencyResolver(container,system);

                system.ActorOf(propsResolver.Create<TypedWorker>(), "Worker1");
                system.ActorOf(propsResolver.Create<TypedWorker>(), "Worker2");
                system.ActorOf(propsResolver.Create<TypedWorker>(), "Worker3");
                system.ActorOf(propsResolver.Create<TypedWorker>(), "Worker4");


                //system.ActorOf(system.Props<TypedWorker>(), "Worker1");
                //system.ActorOf(system.Props<TypedWorker>(), "Worker2");
                //system.ActorOf(system.Props<TypedWorker>(), "Worker3");
                //system.ActorOf(system.Props<TypedWorker>(), "Worker4");

                //system.ActorOf<TypedWorker>("Worker1");
                //system.ActorOf<TypedWorker>("Worker2");
                //system.ActorOf<TypedWorker>("Worker3");
                //system.ActorOf<TypedWorker>("Worker4");

                var hashGroup = system.ActorOf(Props.Empty.WithRouter(new ConsistentHashingGroup(config)));

                Task.Delay(500).Wait();
                Console.WriteLine("Sending Messages");
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

            
            using (var system = ActorSystem.Create("MySystem"))
            {
                NinjectDependencyResolver propsResolver = 
                    new NinjectDependencyResolver(container,system);

                system.ActorOf(propsResolver.Create<TypedWorker>(), "Worker1");
                system.ActorOf(propsResolver.Create<TypedWorker>(), "Worker2");
                system.ActorOf(propsResolver.Create<TypedWorker>(), "Worker3");
                system.ActorOf(propsResolver.Create<TypedWorker>(), "Worker4");

                //system.ActorOf<TypedWorker>("Worker1");
                //system.ActorOf<TypedWorker>("Worker2");
                //system.ActorOf<TypedWorker>("Worker3");
                //system.ActorOf<TypedWorker>("Worker4");

                var hashGroup = system.ActorOf(Props.Empty.WithRouter(new ConsistentHashingGroup(config)));

                Task.Delay(500).Wait();
                Console.WriteLine("Sending Messages");

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


            using (var system = ActorSystem.Create("MySystem"))
            {
                AutoFacDependencyResolver propsResolver = 
                    new AutoFacDependencyResolver(container, system);


                system.ActorOf(propsResolver.Create<TypedWorker>(), "Worker1");
                system.ActorOf(propsResolver.Create<TypedWorker>(), "Worker2");
                system.ActorOf(propsResolver.Create<TypedWorker>(), "Worker3");
                system.ActorOf(propsResolver.Create<TypedWorker>(), "Worker4");

                //system.ActorOf<TypedWorker>("Worker1");
                //system.ActorOf<TypedWorker>("Worker2");
                //system.ActorOf<TypedWorker>("Worker3");
                //system.ActorOf<TypedWorker>("Worker4");

                var hashGroup = system.ActorOf(Props.Empty.WithRouter(new ConsistentHashingGroup(config)));

                Task.Delay(500).Wait();
                Console.WriteLine("Sending Messages");

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
    }
}
