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
using My = Fauux.Banque.Harness.Properties.Resources;

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

            var config = ConfigurationFactory.ParseString(My.HashPoolWOResizer);

            using (var system = ActorSystem.Create("MySystem"))
            {

                //var resizer = new DefaultResizer(3, 5, pressureThreshold: 1, rampupRate: 0.1d, backoffRate: 0.0d,
                //messagesPerResize: 1, backoffThreshold: 0.0d);
                var pool = new ConsistentHashingPool(config);
                pool.NrOfInstances = 10;

                var router = system.ActorOf(Props.Create<TypedWorker>().WithRouter(pool));

                Task.Delay(500).Wait();

                for (var i = 0; i < 5; i++)
                {
                    for (var j = 0; j < 7; j++)
                    {

                        TypedActorMessage msg = new TypedActorMessage { Id = j, Name = Guid.NewGuid().ToString() };
                        AnotherMessage ms = new AnotherMessage { Id = j, Name = msg.Name };

                        var envelope = new ConsistentHashableEnvelope(ms, msg.Id);

                        router.Tell(msg);
                        router.Tell(envelope);
                       

                    }
                }

            }
            Console.ReadLine();
        }
        static void ContainerWindsor()
        {
            var config = ConfigurationFactory.ParseString(My.HashPoolWOResizer);

            using (var system = ActorSystem.Create("MySystem",config))
            {
                IWindsorContainer container = new WindsorContainer();
                container.Register(Component.For<TypedWorker>().Named("TypedWorker").LifestyleTransient());

                var pool = new ConsistentHashingPool(config);
                pool.NrOfInstances = 10;

                WindsorDependencyResolver propsResolver = 
                    new WindsorDependencyResolver(container,system);

                var router = system.ActorOf(propsResolver.Create<TypedWorker>().WithRouter(pool));

                Task.Delay(500).Wait();
                Console.WriteLine("Sending Messages");
                for (var i = 0; i < 5; i++)
                {
                    for (var j = 0; j < 7; j++)
                    {

                        TypedActorMessage msg = new TypedActorMessage { Id = j, Name = Guid.NewGuid().ToString() };
                        AnotherMessage ms = new AnotherMessage { Id = j, Name = msg.Name };

                        var envelope = new ConsistentHashableEnvelope(ms, msg.Id);

                        router.Tell(msg);
                        router.Tell(envelope);

                    }
                }
            }
            
            Console.ReadLine();
        }
        static void ContainerNinject()
        {
            var config = ConfigurationFactory.ParseString(My.HashPoolWOResizer);

            Ninject.IKernel container = new Ninject.StandardKernel();
            container.Bind<TypedWorker>().To(typeof(TypedWorker));

            
            using (var system = ActorSystem.Create("MySystem"))
            {
                NinjectDependencyResolver propsResolver = 
                    new NinjectDependencyResolver(container,system);

                var pool = new ConsistentHashingPool(config);
                pool.NrOfInstances = 10;

                var router = system.ActorOf(propsResolver.Create<TypedWorker>().WithRouter(pool));

                Task.Delay(500).Wait();
                Console.WriteLine("Sending Messages");

                for (var i = 0; i < 5; i++)
                {
                    for (var j = 0; j < 7; j++)
                    {

                        TypedActorMessage msg = new TypedActorMessage { Id = j, Name = Guid.NewGuid().ToString() };
                        AnotherMessage ms = new AnotherMessage { Id = j, Name = msg.Name };

                        var envelope = new ConsistentHashableEnvelope(ms, msg.Id);

                        router.Tell(msg);
                        router.Tell(envelope);

                    }
                }
            }

            
            Console.ReadLine();
        }
        static void ContainerAutoFac()
        {
            var config = ConfigurationFactory.ParseString(My.HashPoolWOResizer);
            var builder = new Autofac.ContainerBuilder();
            builder.RegisterType<TypedWorker>();

            Autofac.IContainer container = builder.Build();


            using (var system = ActorSystem.Create("MySystem"))
            {
                AutoFacDependencyResolver propsResolver = 
                    new AutoFacDependencyResolver(container, system);
                
                var pool = new ConsistentHashingPool(config);
                
                var router = system.ActorOf(propsResolver.Create<TypedWorker>().WithRouter(pool));

                Task.Delay(500).Wait();
                Console.WriteLine("Sending Messages");

                for (var i = 0; i < 5; i++)
                {
                    for (var j = 0; j < 7; j++)
                    {

                        TypedActorMessage msg = new TypedActorMessage { Id = j, Name = Guid.NewGuid().ToString() };
                        AnotherMessage ms = new AnotherMessage { Id = j, Name = msg.Name };

                        var envelope = new ConsistentHashableEnvelope(ms, msg.Id);

                        router.Tell(msg);
                        router.Tell(envelope);

                    }
                }
            }
            
            Console.ReadLine();
        }
    }
}
