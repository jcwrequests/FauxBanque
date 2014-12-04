using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Castle.Windsor;

namespace Fauux.Banque.Harness
{
    public class ApplicationConfig : IApplicationContext
    {
        //Need to Create Implementation of the applicatonContext
        private IWindsorContainer container;
        private Akka.Configuration.Config config;
        

        public ApplicationConfig(IWindsorContainer container)
        {
            // TODO: Complete member initialization
            this.container = container;
        }

        public ApplicationConfig(IWindsorContainer container, Akka.Configuration.Config config)
        {
            // TODO: Complete member initialization
            this.container = container;
            this.config = config;
        }
        public ActorSystem actorSystem(string SystemName)
        {
            
            var system = ActorSystem.Create(SystemName,config);
            system.RegisterExtension((IExtensionId)DIExtension.DIExtensionProvider);
        


            DIExtension.DIExtensionProvider.Get(system).Initialize(this);
            return system;
        }                                                                                                

        public Type GetType(string ActorName)
        {
            return 
                container.
                Kernel.
                GetAssignableHandlers(typeof(object)).
                Where(handler => handler.ComponentModel.Name.Equals(ActorName, StringComparison.InvariantCultureIgnoreCase)).
                Select(handler => handler.ComponentModel.Implementation).
                FirstOrDefault();
        }

        public Func<ActorBase> CreateActor(string ActorName)
        {
           
            return () => (ActorBase)container.Resolve(GetType(ActorName));
        }


         
    }

    public interface IApplicationContext
    {
        Type GetType(string ActorName);
        Func<ActorBase> CreateActor(string ActorName);
    }

    public class DIExt : IExtension
    {
        private IApplicationContext applicationContext;

        public void Initialize(IApplicationContext applicationContext)
        {
            this.applicationContext = applicationContext;
        }
        public Props Props(String actorName)
        {
            return new Props(typeof(DIActorProducerClass),  new object[] { applicationContext, actorName });
        }

    }
    public class DIExtension : ExtensionIdProvider<DIExt> 
    {
        public static DIExtension DIExtensionProvider = new DIExtension();

        public override DIExt CreateExtension(ExtendedActorSystem system)
        {
            var extension = new DIExt();
            return extension;
        }
    }

    public class DIActorProducerClass : IndirectActorProducer
    {
        private IApplicationContext applicationContext;
        private string actorName;
        readonly Func<ActorBase> myActor;

        public DIActorProducerClass(IApplicationContext applicationContext,
                                    string actorName)
        {
            this.applicationContext = applicationContext;
            this.actorName = actorName;
            this.myActor = applicationContext.CreateActor(actorName);
        }
        public Type ActorType
        {
            get { return this.applicationContext.GetType(this.actorName); }
        }

        public ActorBase Produce()
        {
            return myActor();
        }
        
    }
}
