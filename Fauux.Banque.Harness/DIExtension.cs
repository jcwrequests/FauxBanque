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

        public ApplicationConfig(IWindsorContainer container)
        {
            this.container = container;
        }
        public ActorSystem actorSystem(string SystemName)
        {
            var system = ActorSystem.Create(SystemName);
            system.RegisterExtension((IExtensionId)DIExtension.DIExtensionProvider);
            DIExtension.DIExtensionProvider.Get(system).initialize(this);
            return system;
        }                                                                                                

        public Type GetType(string ActorName)
        {
            return Type.GetType(ActorName);
        }

        public ActorBase CreateActor(string ActorName)
        {
            Type actorType =
                container.
                Kernel.
                GetAssignableHandlers(typeof(object)).
                Where(handler => handler.ComponentModel.Name.Equals(ActorName, StringComparison.InvariantCultureIgnoreCase)).
                Select(handler => handler.ComponentModel.Implementation).
                FirstOrDefault();
            
            return (ActorBase)container.Resolve(actorType);
        }
    }

    public interface IApplicationContext
    {
        Type GetType(string ActorName);
        ActorBase CreateActor(string ActorName);
    }

    public class DIExt : IExtension
    {
        private IApplicationContext applicationContext;

        public void initialize(IApplicationContext applicationContext)
        {
            this.applicationContext = applicationContext;
        }
        public Props props(String actorName)
        {
            return new Props(typeof(DIActorProducerClass), new object[] { applicationContext, actorName });
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
        readonly ActorBase myActor;

        public DIActorProducerClass(IApplicationContext applicationContext,
                                    string actorName)
        {
            this.applicationContext = applicationContext;
            this.actorName = actorName;
            this.myActor = applicationContext.CreateActor(actorName);
        }
        public Type ActorType
        {
            get { return myActor.GetType(); }
        }

        public ActorBase Produce()
        {
            return myActor;
        }
    }
}
