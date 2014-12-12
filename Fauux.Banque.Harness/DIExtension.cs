﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Castle.Windsor;
using Autofac.Builder;
using Autofac.Core;
using Autofac;
using System.Collections.Concurrent;

namespace Fauux.Banque.Harness
{
    public static class ActorSystemExtensions
    {
        public static void AddDependencyResolver(this ActorSystem system, IDependencyResolver ext)
        {
            if (system == null) throw new ArgumentNullException("system");
            system.RegisterExtension((IExtensionId)DIExtension.DIExtensionProvider);
            DIExtension.DIExtensionProvider.Get(system).Initialize(ext);
        }

        
    }
    public static class StringExtensions
    {
        public static Type GetTypeValue(this string typeName)
        {
            var firstTry = Type.GetType(typeName);
            Func<Type> searchForType = () =>
                {
                    return
                    AppDomain.
                        CurrentDomain.
                        GetAssemblies().
                        SelectMany(x => x.GetTypes()).
                        Where(t => t.Name.Equals(typeName)).
                        FirstOrDefault();
                };
            return firstTry ?? searchForType();
        }
    }
    public class NinjectDependencyResolver : IDependencyResolver
    {
        Ninject.IKernel container;
        private ConcurrentDictionary<string, Type> typeCache;

        public NinjectDependencyResolver(Ninject.IKernel container)
        {
            if (container == null) throw new ArgumentNullException("container");
            this.container = container;
            typeCache = new ConcurrentDictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
        }
        
       

        public Type GetType(string ActorName)
        {
            if (!typeCache.ContainsKey(ActorName))
                typeCache.TryAdd(ActorName, ActorName.GetTypeValue());

            return typeCache[ActorName];
        }

        public Func<ActorBase> CreateActorFactory(string ActorName)
        {
            return () =>
            {
                Type actorType = this.GetType(ActorName);
                
                return (ActorBase)container.GetService(actorType);
            };
            
        }
    }
    public class AutoFacDependencyResolver : IDependencyResolver
    {
        private IContainer container;
        private ConcurrentDictionary<string, Type> typeCache;

        public AutoFacDependencyResolver(IContainer container)
        {
            this.container = container;
            typeCache = new ConcurrentDictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
        }

        public Type GetType(string ActorName)
        {
            if (!typeCache.ContainsKey(ActorName))
                typeCache.TryAdd(ActorName, 
                ActorName.GetTypeValue() ??
                container.
                ComponentRegistry.
                Registrations.
                Where(registration => registration.Activator.LimitType.
                    Name.Equals(ActorName, StringComparison.InvariantCultureIgnoreCase)).
                    Select(regisration => regisration.Activator.LimitType).
                    FirstOrDefault());

            return typeCache[ActorName];
            
        }

        public Func<ActorBase> CreateActorFactory(string ActorName)
        {
            return () =>
            {
                Type actorType = this.GetType(ActorName);
                return (ActorBase)container.Resolve(actorType);
            };
        }
    }
    public class WindsorDependencyResolver : IDependencyResolver
    {
        private IWindsorContainer container;
        private ConcurrentDictionary<string, Type> typeCache;

        public WindsorDependencyResolver(IWindsorContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            this.container = container;
            typeCache = new ConcurrentDictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
        }

                                                                                             

        public Type GetType(string ActorName)
        {
            if (!typeCache.ContainsKey(ActorName))
   
                typeCache.TryAdd(ActorName, 
                                 ActorName.GetTypeValue() ??
                                 container.
                                 Kernel.
                                 GetAssignableHandlers(typeof(object)).
                                 Where(handler => handler.ComponentModel.Name.Equals(ActorName, StringComparison.InvariantCultureIgnoreCase)).
                                 Select(handler => handler.ComponentModel.Implementation).
                                 FirstOrDefault());

            return typeCache[ActorName];
        }

        public Func<ActorBase> CreateActorFactory(string ActorName)
        {
            return () => (ActorBase)container.Resolve(GetType(ActorName));
        }


         
    }


    public interface IDependencyResolver
    {
        Type GetType(string ActorName);
        Func<ActorBase> CreateActorFactory(string ActorName);
    }

    public class DIExt : IExtension
    {
        private IDependencyResolver dependencyResolver;

        public void Initialize(IDependencyResolver dependencyResolver)
        {
            this.dependencyResolver = dependencyResolver;
        }
        public Props Props(String actorName)
        {
            return new Props(typeof(DIActorProducerClass),  new object[] { dependencyResolver, actorName });
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
        private IDependencyResolver dependencyResolver;
        private string actorName;
        readonly Func<ActorBase> myActor;

        public DIActorProducerClass(IDependencyResolver dependencyResolver,
                                    string actorName)
        {
            this.dependencyResolver = dependencyResolver;
            this.actorName = actorName;
            this.myActor = dependencyResolver.CreateActorFactory(actorName);
        }
        public Type ActorType
        {
            get { return this.dependencyResolver.GetType(this.actorName); }
        }

        public ActorBase Produce()
        {
            return myActor();
        }
        
    }
}
