
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Faux.Banque.Domain;
using Faux.Banque.Domain.Storage;
using Faux.Banque.Domain.Interfaces;

namespace Faux.Banque.Domain.Tests.EventStore
{
    public class InMemoryEventStore : IEventStore
    {
        private Dictionary<IIdentity, EventStream> store;

        public InMemoryEventStore()
        {
            store = new Dictionary<IIdentity, EventStream>();
        }

        public EventStream LoadEventStream(IIdentity id)
        {
            if (!store.ContainsKey(id)) return new EventStream();
            return store[id];
        }

        public void AppendToStream(IIdentity id, long expectedVersion, ICollection<Interfaces.IEvent> events)
        {
            var stream = LoadEventStream(id);
            stream.Events.AddRange(events);
            stream.Version = expectedVersion;
            this.store[id] = stream;
        }
    }
}
