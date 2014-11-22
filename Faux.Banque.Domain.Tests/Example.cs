﻿using Akka.Actor;
using Akka.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Faux.Banque.Domain.Tests
{
    public class HashableMessage : ConsistentHashable
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public object ConsistentHashKey
        {
            get { return Id; }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Id, Name);
        }
    }
    public class Worker : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            Console.WriteLine("{0} received {1}", Self.Path.Name, message);
        }
    }
}
