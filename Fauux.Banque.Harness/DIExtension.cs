using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Castle.Windsor;
using Autofac.Builder;
using Autofac.Core;
using Autofac;

namespace Fauux.Banque.Harness
{
    public class NijectConfiuration : IContainerConfiguration
    {
        public NijectConfiuration(Ninject.IKernel container)
        {
            if (container == null) throw new ArgumentNullException("container");
            this.container = container;
        }
        Ninject.IKernel container;
        public Type GetType(string ActorName)
        {
            return
            this.
                container.
                GetBindings(typeof(object))
                .Where(binding => binding.Target.GetType().
                    Name.Equals(ActorName, StringComparison.InvariantCultureIgnoreCase)).
                    Select(binding => binding.Target.GetType()).
                    FirstOrDefault();
        }

        public Func<ActorBase> CreateActor(string ActorName)
        {
            return () =>
            {
                Type actorType = this.GetType(ActorName);
                return (ActorBase)container.GetService(actorType);
            };
            
        }
    }
    public class AutoFacConfiguration : IContainerConfiguration
    {
        private IContainer container;

        public AutoFacConfiguration(IContainer container)
        {
            this.container = container;
        }
        public Type GetType(string ActorName)
        {
            return 
                container.
                ComponentRegistry.
                Registrations.
                Where(registration => registration.Target.GetType().
                    Name.Equals(ActorName, StringComparison.InvariantCultureIgnoreCase)).
                    Select(regisration => regisration.Target.GetType()).
                    FirstOrDefault();
            
        }
        public ActorSystem actorSystem(string SystemName)
        {
            var system = ActorSystem.Create(SystemName);
            system.RegisterExtension((IExtensionId)DIExtension.DIExtensionProvider);

            DIExtension.DIExtensionProvider.Get(system).Initialize(this);
            return system;
        }           
        public Func<ActorBase> CreateActor(string ActorName)
        {
            return () =>
            {
                Type actorType = this.GetType(ActorName);
                return (ActorBase)container.Resolve(actorType);
            };
        }
    }
    public class WindsorConfiguration : IContainerConfiguration
    {
        private IWindsorContainer container;

        public WindsorConfiguration(IWindsorContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            this.container = container;
        }

        public ActorSystem actorSystem(string SystemName)
        {
            if (SystemName == null) throw new ArgumentNullException("SystemName");
            var system = ActorSystem.Create(SystemName);
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


    public interface IContainerConfiguration
    {
        Type GetType(string ActorName);
        Func<ActorBase> CreateActor(string ActorName);
    }

    public class DIExt : IExtension
    {
        private IContainerConfiguration applicationContext;

        public void Initialize(IContainerConfiguration applicationContext)
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
        private IContainerConfiguration applicationContext;
        private string actorName;
        readonly Func<ActorBase> myActor;

        public DIActorProducerClass(IContainerConfiguration applicationContext,
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
